using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDotnetDemo.Models;
using TrainReservationSystem.DTO;

public class TrainService : ITrainService
{
    private readonly IMongoCollection<Train> _trainCollection;
    private readonly IMongoCollection<Schedule> _scheduleCollection;

    public TrainService(IOptions<DatabaseSettings> dbSettings)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _trainCollection = mongoDatabase.GetCollection<Train>(dbSettings.Value.TrainsCollectionName);
        _scheduleCollection = mongoDatabase.GetCollection<Schedule>(dbSettings.Value.SchedulesCollectionName);
    }

    public async Task<IEnumerable<Train>> GetAllAsync()
    {
        //get all trains
        var trains = await _trainCollection.Find(_ => true).ToListAsync();
        return trains;
    }

    public async Task<IEnumerable<ActiveTrainsForBooking>> GetActiveTrains()
    {
        //gets all active trains available for booking . used when placing a booking
        var activeTrainsList = new List<ActiveTrainsForBooking>();
        var trains = await _trainCollection.Find(t => t.IsActive == true).ToListAsync();

        foreach (var train in trains)
        {
            //creates a list of active trains with custom properties
            ActiveTrainsForBooking activeTrain = new ActiveTrainsForBooking();
            var schedule = await _scheduleCollection.Find(s => s.Id == train.Schedule).FirstOrDefaultAsync();
            activeTrain.Id = train.Id;
            activeTrain.ScheduleId = schedule.Id;
            activeTrain.TrainName = train.TrainName;
            activeTrain.Route = schedule.stopStations;
            activeTrain.LuxurySeatCount = train.LuxurySeatCount;
            activeTrain.EconomySeatCount = train.EconomySeatCount;
            activeTrain.AvailableLuxurySeats = train.LuxurySeatCount - train.OccupiedLuxurySeatCount;
            activeTrain.AvailableEconomySeats = train.EconomySeatCount - train.OccupiedEconomySeatCount;
            activeTrain.OperatingDays = schedule.OperatingDays;
            activeTrainsList.Add(activeTrain);
        }

        return activeTrainsList;
    }

    public async Task<IEnumerable<ActiveTrainsForBooking>> GetActiveTrainsForRoute(string route)
    {
        //gets all active trains available for booking . used when placing a booking
        var activeTrainsList = new List<ActiveTrainsForBooking>();
        var trains = await _trainCollection.Find(t => t.IsActive == true).ToListAsync();

        foreach (var train in trains)
        {
            //creates a list of active trains with custom properties
            ActiveTrainsForBooking activeTrain = new ActiveTrainsForBooking();
            var schedule = await _scheduleCollection.Find(s => s.Id == train.Schedule).FirstOrDefaultAsync();

            if (schedule.Route != route)
                continue;

            activeTrain.Id = train.Id;
            activeTrain.ScheduleId = schedule.Id;
            activeTrain.TrainName = train.TrainName;
            activeTrain.Route = schedule.stopStations;
            activeTrain.LuxurySeatCount = train.LuxurySeatCount;
            activeTrain.EconomySeatCount = train.EconomySeatCount;
            activeTrain.AvailableLuxurySeats = train.LuxurySeatCount - train.OccupiedLuxurySeatCount;
            activeTrain.AvailableEconomySeats = train.EconomySeatCount - train.OccupiedEconomySeatCount;
            activeTrain.OperatingDays = schedule.OperatingDays;
            activeTrainsList.Add(activeTrain);
        }

        return activeTrainsList;
    }

    public async Task<Train> GetByIdAsync(string id)
    {
        //get train by id
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        return train;
    }

    public async Task<string> CreateAsync(Train train)
    {
        //create a train
        //set default values
        train.IsActive = false;
        train.Reservations = new List<string>();
        train.Schedule = null;
        train.OccupiedEconomySeatCount = 0;
        train.OccupiedLuxurySeatCount = 0;

        //default seat counts
        if (train.LuxurySeatCount == 0 || train.LuxurySeatCount == null)
            train.LuxurySeatCount = 50;

        if (train.EconomySeatCount == 0 || train.EconomySeatCount == null)
            train.EconomySeatCount = 50;


        await _trainCollection.InsertOneAsync(train);
        return "Train created successfully";
    }

    public async Task<string> UpdateAsync(string id, Train newTrain)
    {
        //updates a train
        var existingTrain = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (existingTrain == null)
            return "Train not found";

        if (existingTrain.Reservations != null && existingTrain.Reservations.Count > 0)
            return "Cannot update reserved trains";

        var filter = Builders<Train>.Filter.Eq(t => t.Id, id);
        var update = Builders<Train>.Update
            .Set(t => t.TrainName, newTrain.TrainName)
            .Set(t => t.LuxurySeatCount, newTrain.LuxurySeatCount)
            .Set(t => t.EconomySeatCount, newTrain.EconomySeatCount);

        await _trainCollection.UpdateOneAsync(filter, update);
        return "Train updated successfully";
    }

    public async Task<string> AddScheduleAsync(string id, string scheduleId)
    {
        //assign a schedule to a train
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        if (train.Reservations != null && train.Reservations.Count > 0)
            return "Cannot change schedule of reserved trains";

        var schedule = await _scheduleCollection.Find(s => s.Id == scheduleId).FirstOrDefaultAsync();
        if (schedule == null)
            return "Schedule not found";

        train.IsActive = true;
        train.Schedule = scheduleId;
        await _trainCollection.ReplaceOneAsync(t => t.Id == id, train);
        return "Schedule added to the train successfully";
    }

    public async Task<string> ActivateTrainAsync(string id)
    {
        //activates a train
        //only train with a schedule can be activated
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        if (train.Schedule == null)
            return "Please assign a schedule first";

        train.IsActive = true;
        await _trainCollection.ReplaceOneAsync(t => t.Id == id, train);
        return "Train activated successfully";
    }

    public async Task<string> DeactivateTrainAsync(string id)
    {
        //deactivate train
        //only unreserved trains can be deactivated
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        if (train.Reservations != null && train.Reservations.Count > 0)
            return "Cannot deactivate train with reservations";

        train.Schedule = null;
        train.IsActive = false;
        train.OccupiedLuxurySeatCount = 0;
        train.OccupiedEconomySeatCount = 0;

        await _trainCollection.ReplaceOneAsync(t => t.Id == id, train);
        return "Train deactivated successfully";
    }

    public async Task<string> DeleteAsync(string id)
    {
        //delete train
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        if (train.IsActive == true)
            return "Cannot delete an active train";

        await _trainCollection.DeleteOneAsync(t => t.Id == id);
        return "Train deleted successfully";
    }
}
