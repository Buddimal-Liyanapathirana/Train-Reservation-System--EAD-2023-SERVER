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


    public ReservationService(IOptions<DatabaseSettings> dbSettings)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _reservationCollection = mongoDatabase.GetCollection<Reservation>(dbSettings.Value.ReservationsCollectionName);
        _trainCollection = mongoDatabase.GetCollection<Train>(dbSettings.Value.TrainsCollectionName);
        _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Value.UsersCollectionName);
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        var reservations = await _reservationCollection.Find(_ => true).ToListAsync();
        return reservations;
    }

    public async Task<Reservation> GetByIdAsync(string id)
    {
        var reservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        return reservation;
    }

    public async Task<string> CreateAsync(Reservation reservation)
    {
        // Check if UserNIC and TrainId are valid existing documents
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

        reservation.CreatedOn = DateTime.Now;
       
        if ((reservation.ReservedOn - reservation.CreatedOn).TotalDays < 5)
        {
            return "Cannot Place reservations within 5 days";
        }

        var result = await OccupyTrainSeats(reservation.TrainId, reservation.LuxurySeats, reservation.EconomySeats);
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
            await _reservationCollection.InsertOneAsync(reservation);
            // Update Train and User collections 
            await UpdateTrainAndUserCollections(train.Id, user.NIC, reservation.Id);
        }
        return "Reservation created successfully";
    }

    public async Task<string> UpdateAsync(string id, Reservation reservation)
    {
        // Check if the reservation exists
        var existingReservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (existingReservation == null)
            return "Reservation not found";

        if ((reservation.ReservedOn - DateTime.Now).TotalDays < 5)
        {
            return "Cannot update within 5 days of the reservation date";
        }

        var result = await UpdateOccupiedTrainSeats(reservation.TrainId, reservation.LuxurySeats, reservation.EconomySeats , existingReservation.LuxurySeats,existingReservation.EconomySeats);
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
            var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
            await _reservationCollection.ReplaceOneAsync(filter, existingReservation);

            return "Reservation updated successfully";
        }
    }

    public async Task<string> DeleteAsync(string id)
    {
        var reservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (reservation == null)
            return "Reservation not found";

        if ((reservation.ReservedOn - DateTime.Now).Days < 5)
            return "Cannot delete reservation within 5 days of the reservation date";

        await _reservationCollection.DeleteOneAsync(r => r.Id == id);
        await RemoveReservationIdsFromTrainAndUserCollections(reservation.TrainId, reservation.UserNIC, id);
        await DeOccupyTrainSeats(reservation.TrainId, reservation.LuxurySeats, reservation.EconomySeats);

        return "Reservation deleted successfully";
    }

    //updating reservation Ids from Train and User collections
    private async Task UpdateTrainAndUserCollections(string trainId, string userNIC, string reservationId)
    {
        var filterTrain = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var updateTrain = Builders<Train>.Update.Push(t => t.Reservations, reservationId);
        await _trainCollection.UpdateOneAsync(filterTrain, updateTrain);

        var filterUser = Builders<User>.Filter.Eq(u => u.NIC, userNIC);
        var updateUser = Builders<User>.Update.Push(u => u.ReservationIds, reservationId);
        await _userCollection.UpdateOneAsync(filterUser, updateUser);
    }

    //removing reservation Ids from Train and User collections
    private async Task RemoveReservationIdsFromTrainAndUserCollections(string trainId, string userNIC, string reservationId)
    {
        var filterTrain = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var updateTrain = Builders<Train>.Update.Pull(t => t.Reservations, reservationId);
        await _trainCollection.UpdateOneAsync(filterTrain, updateTrain);

        var filterUser = Builders<User>.Filter.Eq(u => u.NIC, userNIC);
        var updateUser = Builders<User>.Update.Pull(u => u.ReservationIds, reservationId);
        await _userCollection.UpdateOneAsync(filterUser, updateUser);
    }

    public async Task<int> OccupyTrainSeats(string trainId, int luxurySeats, int economySeats)
    {
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
}
