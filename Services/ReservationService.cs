using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDotnetDemo.Models;

public class ReservationService : IReservationService
{
    private readonly IMongoCollection<Reservation> _reservationCollection;
    private readonly IMongoCollection<Train> _trainCollection;
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<Schedule> _scheduleCollection;


    public ReservationService(IOptions<DatabaseSettings> dbSettings)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _reservationCollection = mongoDatabase.GetCollection<Reservation>(dbSettings.Value.ReservationsCollectionName);
        _trainCollection = mongoDatabase.GetCollection<Train>(dbSettings.Value.TrainsCollectionName);
        _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Value.UsersCollectionName);
        _scheduleCollection = mongoDatabase.GetCollection<Schedule>(dbSettings.Value.SchedulesCollectionName);
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        //get all reservations
        var reservations = await _reservationCollection.Find(_ => true).ToListAsync();
        return reservations;
    }


    public async Task<Reservation> GetByIdAsync(string id)
    {
        //get reservation by id
        var reservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        return reservation;
    }

    public async Task<IEnumerable<Reservation>> GetByUserNicAsync(string userNic)
    {
        //get reservations by user
        var reservations = await _reservationCollection.Find(r => r.UserNIC == userNic).ToListAsync();
        return reservations;
    }

    public async Task<string> CreateRequestAsync(Reservation reservation)
    {
        // Creating the reservation request
        Reservation resReq = new Reservation();

        var user = await _userCollection.Find(u => u.NIC == reservation.RequestedBy).FirstOrDefaultAsync();

        if (user == null)
            return "Invalid User";

        if (reservation.LuxurySeats + reservation.EconomySeats < 1)
            return "Please pick a valid number of seats";

        if (reservation.RequestedToDate == null)
            return "Please pick a valid date";

        if (reservation.LuxurySeats == null)
            reservation.LuxurySeats = 0;

        if (reservation.EconomySeats == null)
            reservation.EconomySeats = 0;

        if ((reservation.RequestedToDate - DateTime.Now).Days < 5)
            return "Cannot request within 5 days";


        resReq.EconomySeats = reservation.EconomySeats;
        resReq.LuxurySeats = reservation.LuxurySeats;
        resReq.isRequested = true;
        resReq.RequestedToDate =  reservation.RequestedToDate;
        resReq.RequestedBy = reservation.RequestedBy;

        await _reservationCollection.InsertOneAsync(resReq);
        return "Reservation request created successfully";
    }


    public async Task<string> CreateAsync(Reservation reservation)
    {
        // Creating the reservation
        var user = await _userCollection.Find(u => u.NIC == reservation.UserNIC).FirstOrDefaultAsync();
        var train = await _trainCollection.Find(t => t.Id == reservation.TrainId).FirstOrDefaultAsync();

        if (user == null || train == null)
            return "Invalid User NIC or Train";

        if (user.Role == "BACK_OFFICER"||user.Role=="TRAVEL_AGENT")
            return "Invalid user role";

        if (train.IsActive == false)
            return "Cannot reserve inactive trains";

        if (user.IsActive==false)
            return "Cannot create reservation for inactive user";

        if (user.ReservationIds?.Count >= 4)
            return "User has reached the maximum limit of reservations";

        if(reservation.LuxurySeats + reservation.EconomySeats <1)
            return "Please pick a valid number of seats";

        reservation.CreatedOn = DateTime.Now;
        reservation.isCompleted = false;
        reservation.completedTrain = null;
        reservation.isRequested = false;
       
        if ((reservation.ReservedOn - reservation.CreatedOn).TotalDays < 5)
        {
            return "Cannot Place reservations within 5 days";
        }

        var result = await OccupyTrainSeats(reservation.TrainId, reservation.LuxurySeats, reservation.EconomySeats );
        if (result == -1)
        {
            return "Luxury seat capacity exceeded . Reduce number of seats";
        }
        else if (result == -2)
        {
            return "Economy seat capacity exceeded . Reduce number of seats";
        }
        else
        {
            double totalFare = await CalculateToralFare(train.Schedule , reservation.LuxurySeats , reservation.EconomySeats, reservation.startStation, reservation.endStation);
            reservation.TotalFare = totalFare;
            await _reservationCollection.InsertOneAsync(reservation);
            await UpdateTrainAndUserCollections(train.Id, user.NIC, reservation.Id);
        }
        return "Reservation created successfully";
    }

    public async Task<string> UpdateAsync(string id, Reservation reservation)
    {
        //update reservations
        var existingReservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (existingReservation == null)
            return "Reservation not found";

        if (reservation.isRequested)
        {
            return "Cannot update a request";
        }

        var train = await _trainCollection.Find(t => t.Id == existingReservation.TrainId).FirstOrDefaultAsync();

        if ((reservation.ReservedOn - DateTime.Now).TotalDays < 5)
        {
            return "Cannot update within 5 days of the reservation date";
        }

        var result = await UpdateOccupiedTrainSeats(existingReservation.TrainId, reservation.LuxurySeats, reservation.EconomySeats , existingReservation.LuxurySeats,existingReservation.EconomySeats);
        if (result == -1)
        {
            return "Luxury seat capacity exceeded . Reduce number of seats";
        }
        else if (result == -2)
        {
            return "Economy seat capacity exceeded . Reduce number of seats";
        }
        else
        {
            double newTotalFare = await CalculateToralFare(train.Schedule, reservation.LuxurySeats, reservation.EconomySeats, reservation.startStation, reservation.endStation);

            var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
            var update = Builders<Reservation>.Update
                .Set(s => s.LuxurySeats, reservation.LuxurySeats)
                .Set(s => s.EconomySeats, reservation.EconomySeats)
                .Set(s => s.startStation, reservation.startStation)
                .Set(s => s.endStation, reservation.endStation)
                .Set(s => s.ReservedOn, reservation.ReservedOn)
                .Set(s => s.TotalFare, newTotalFare);

            await _reservationCollection.UpdateOneAsync(filter, update);

            return "Reservation updated successfully";
        }
    }

    public async Task<string> DeleteAsync(string id)
    {
        //delete a reservation
        var reservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (reservation == null)
            return "Reservation not found";

        if (reservation.isRequested==true)
        {
            await _reservationCollection.DeleteOneAsync(r => r.Id == id);
            return "Reservation request deleted successfully";
        }

        if ((reservation.ReservedOn - DateTime.Now).Days < 5)
            return "Cannot delete reservation within 5 days of the reservation date";

        await _reservationCollection.DeleteOneAsync(r => r.Id == id);

        if(reservation.isCompleted)
            return "Reservation deleted successfully";

        await RemoveReservationIdsFromTrainAndUserCollections(reservation.TrainId, reservation.UserNIC, id);
        await DeOccupyTrainSeats(reservation.TrainId, reservation.LuxurySeats, reservation.EconomySeats);

        return "Reservation deleted successfully";
    }

    public async Task<string> CompleteReservation(string id)
    {
        //marks reservation as complete . this is displayed in reservation history
        var existingReservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (existingReservation == null)
            return "Reservation not found";

        var train = await _trainCollection.Find(t => t.Id == existingReservation.TrainId).FirstOrDefaultAsync();

        existingReservation.isCompleted = true;
        existingReservation.completedTrain = train.TrainName;


        var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
        var update = Builders<Reservation>.Update
            .Set(s => s.isCompleted, existingReservation.isCompleted)
            .Set(s => s.completedTrain, existingReservation.completedTrain);

        await _reservationCollection.UpdateOneAsync(filter, update);

        await RemoveReservationIdsFromTrainAndUserCollections(existingReservation.TrainId, existingReservation.UserNIC, id);
        await DeOccupyTrainSeats(existingReservation.TrainId, existingReservation.LuxurySeats, existingReservation.EconomySeats);

        return "Reservation is completed successfully";
    }

    
    private async Task UpdateTrainAndUserCollections(string trainId, string userNIC, string reservationId)
    {
        //updating reservation Ids list from Train and User collections
        var filterTrain = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var updateTrain = Builders<Train>.Update.Push(t => t.Reservations, reservationId);
        await _trainCollection.UpdateOneAsync(filterTrain, updateTrain);

        var filterUser = Builders<User>.Filter.Eq(u => u.NIC, userNIC);
        var updateUser = Builders<User>.Update.Push(u => u.ReservationIds, reservationId);
        await _userCollection.UpdateOneAsync(filterUser, updateUser);
    }

    
    private async Task RemoveReservationIdsFromTrainAndUserCollections(string trainId, string userNIC, string reservationId)
    {
        //removing reservation Ids from Train and User reservation list
        var filterTrain = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var updateTrain = Builders<Train>.Update.Pull(t => t.Reservations, reservationId);
        await _trainCollection.UpdateOneAsync(filterTrain, updateTrain);

        var filterUser = Builders<User>.Filter.Eq(u => u.NIC, userNIC);
        var updateUser = Builders<User>.Update.Pull(u => u.ReservationIds, reservationId);
        await _userCollection.UpdateOneAsync(filterUser, updateUser);
    }

    public async Task<int> OccupyTrainSeats(string trainId, int luxurySeats, int economySeats)
    {
        //occupy seats from train upon creation of a reservation
        var train = await _trainCollection.Find(t => t.Id == trainId).FirstOrDefaultAsync();

        int maxLuxurySeats = train.LuxurySeatCount;
        int maxEconomySeats = train.EconomySeatCount;

        int UpdatedOccupiedLuxurySeats = train.OccupiedLuxurySeatCount + luxurySeats;
        int UpdatedOccupiedEconomySeats = train.OccupiedEconomySeatCount + economySeats;

        if (UpdatedOccupiedLuxurySeats > maxLuxurySeats)
            return -1;

        if (UpdatedOccupiedEconomySeats > maxEconomySeats)
            return -2;

        var filter = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var update = Builders<Train>.Update
            .Set(t => t.OccupiedLuxurySeatCount, UpdatedOccupiedLuxurySeats)
            .Set(t => t.OccupiedEconomySeatCount,UpdatedOccupiedEconomySeats);

        await _trainCollection.UpdateOneAsync(filter, update);
        return 1;
    }

    public async Task<int> UpdateOccupiedTrainSeats(string trainId, int luxurySeats, int economySeats , int existingLuxurySeats , int existingEconomySeats)
    {
        //update occupied seats from train upon update of a reservation
        var train = await _trainCollection.Find(t => t.Id == trainId).FirstOrDefaultAsync();

        int maxLuxurySeats = train.LuxurySeatCount;
        int maxEconomySeats = train.EconomySeatCount;

        int UpdatedOccupiedLuxurySeats = train.OccupiedLuxurySeatCount + luxurySeats - existingLuxurySeats;
        int UpdatedOccupiedEconomySeats = train.OccupiedEconomySeatCount + economySeats - existingEconomySeats;

        if (UpdatedOccupiedLuxurySeats > maxLuxurySeats)
            return -1;

        if (UpdatedOccupiedEconomySeats > maxEconomySeats)
            return -2;

        var filter = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var update = Builders<Train>.Update
            .Set(t => t.OccupiedLuxurySeatCount, UpdatedOccupiedLuxurySeats)
            .Set(t => t.OccupiedEconomySeatCount, UpdatedOccupiedEconomySeats);

        await _trainCollection.UpdateOneAsync(filter, update);
        return 1;
    }

    public async Task<int> DeOccupyTrainSeats(string trainId, int luxurySeats, int economySeats)
    {
        //Remove occupied seats from train upon deletion or completion of a reservation
        var train = await _trainCollection.Find(t => t.Id == trainId).FirstOrDefaultAsync();

        int UpdatedOccupiedLuxurySeats = train.OccupiedLuxurySeatCount - luxurySeats;
        int UpdatedOccupiedEconomySeats = train.OccupiedEconomySeatCount - economySeats;

        var filter = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var update = Builders<Train>.Update
            .Set(t => t.OccupiedLuxurySeatCount, UpdatedOccupiedLuxurySeats)
            .Set(t => t.OccupiedEconomySeatCount, UpdatedOccupiedEconomySeats);

        await _trainCollection.UpdateOneAsync(filter, update);
        return 1;
    }

    public async Task<double> CalculateToralFare(string scheduleID, int luxurySeats , int economySeats, string startStation , string endStation)
    {
        //calculates total fare for a reservation based on distance and number of seats
        var existingSchedule = await _scheduleCollection.Find(t => t.Id == scheduleID).FirstOrDefaultAsync();
        List<string> stations = existingSchedule.stopStations.ToList();

        //calculates gap between start and end stations
        int startIndex = stations.IndexOf(startStation);
        int endIndex = stations.IndexOf(endStation);
        int stationsBetween = Math.Abs(endIndex - startIndex);

        double luxuryFare = existingSchedule.LuxuryFare;
        double economyFare = existingSchedule.EconomyFare;
        double totalFare = luxuryFare*luxurySeats+economyFare*economySeats;
        return totalFare * stationsBetween;
    }
}
