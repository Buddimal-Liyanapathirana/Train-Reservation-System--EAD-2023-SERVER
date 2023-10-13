using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDotnetDemo.Models;
using TrainReservationSystem.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class ScheduleService : IScheduleService
{
    private readonly IMongoCollection<Schedule> _scheduleCollection;
    private readonly IOptions<DatabaseSettings> _dbSettings;
    private readonly ITrainService _trainService;

    public ScheduleService(IOptions<DatabaseSettings> dbSettings , ITrainService trainService)
    {
        _dbSettings = dbSettings;
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _scheduleCollection = mongoDatabase.GetCollection<Schedule>(dbSettings.Value.SchedulesCollectionName);
        _trainService = trainService;
    }

    public async Task<IEnumerable<Schedule>> GetAllAsync()
    {
        //get all schedules
        var schedules = await _scheduleCollection.Find(_ => true).ToListAsync();
        return schedules;
    }

    public async Task<Schedule> GetByIdAsync(string id)
    {
        //get schedule by id
        var schedule = await _scheduleCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
        return schedule;
    }

    public async Task<string> CreateAsync(Schedule schedule)
    {
        //default operating days
        schedule.OperatingDays = new HashSet<DayOfWeek>
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        };

        //default routes
        schedule.stopStations = Stations.SOUTH;

        if (schedule.Route.Equals("NORTH"))
            schedule.stopStations = Stations.NORTH;

        if (schedule.Route.Equals("UPCOUNTRY"))
            schedule.stopStations = Stations.UPCOUNTRY;

        schedule.DepartureStation = schedule.stopStations.ToArray().FirstOrDefault();
        schedule.ArrivalStation = schedule.stopStations.ToArray().LastOrDefault();

        await _scheduleCollection.InsertOneAsync(schedule);
        return "Schedule created successfully";
    }

    public async Task<string> UpdateAsync(string id, Schedule schedule)
    {
        //updates schedules
        //only schedules that are not occupied by trains can be deleted
        var existingSchedule = await _scheduleCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (existingSchedule == null)
            return "Schedule not found";

        if (!await IsScheduleNotInUse(id))
            return "Cannot update a schedule in use";
        
        var filter = Builders<Schedule>.Filter.Eq(s => s.Id, id);
        var update = Builders<Schedule>.Update
            .Set(s => s.DepartureStation, schedule.DepartureStation)
            .Set(s => s.ArrivalStation, schedule.ArrivalStation)
            .Set(s => s.DepartureTime, schedule.DepartureTime)
            .Set(s => s.ArrivalTime, schedule.ArrivalTime)
            .Set(s => s.LuxuryFare, schedule.LuxuryFare)
            .Set(s => s.EconomyFare, schedule.EconomyFare);


        await _scheduleCollection.UpdateOneAsync(filter, update);
        return "Schedule updated successfully";
    }

    public async Task<string> DeleteAsync(string id)
    {
        //deletes a schedule
        //only schedules that are unoccupied by trains can be deleted
        var existingSchedule = await _scheduleCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (existingSchedule == null)
            return "Schedule not found";

        if (await IsScheduleNotInUse(id))
        {
            await _scheduleCollection.DeleteOneAsync(s => s.Id == id);
            return "Schedule deleted successfully";
        }
        else
        {
            return "Cannot delete a schedule in use";
        }
    }

    public async Task<bool> IsScheduleNotInUse(string id)
    {
        //checks if a schedule is used by a train
        var trains = await _trainService.GetAllAsync();
        bool isAssigned = true;

        foreach (var train in trains)
        {
            if (train.Schedule == id)
                isAssigned = false;
        }
        return isAssigned;
    }
}
