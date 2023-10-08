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
            return "Invalid UserNIC or TrainId";

        if (user.IsActive==false)
            return "Cannot create reservation for inactive user";

        // Check if the user has a maximum of 4 reservations
        if (user.ReservationIds?.Count >= 4)
            return "User has reached the maximum limit of reservations";

        // Set CreatedOn and ReservedOn
        reservation.CreatedOn = DateTime.Now;
       
        if ((reservation.ReservedOn - reservation.CreatedOn).TotalDays < 5)
        {
            return "Cannot Place reservations within 5 days";
        }

        // Insert reservation
        await _reservationCollection.InsertOneAsync(reservation);

        // Update Train and User collections with the reservation Id
        await UpdateTrainAndUserCollections(train.Id, user.NIC, reservation.Id);

        return "Reservation created successfully";
    }

    public async Task<string> UpdateAsync(string id, Reservation reservation)
    {
        // Check if the reservation exists
        var existingReservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (existingReservation == null)
            return "Reservation not found";

        // Check if it's at least 5 days before the reservation date
        if ((existingReservation.ReservedOn - DateTime.Now).Days < 5)
            return "Cannot update reservation within 5 days of the reservation date";

        // Update EconomySeats and LuxurySeats
        existingReservation.EconomySeats = reservation.EconomySeats;
        existingReservation.LuxurySeats = reservation.LuxurySeats;

        // Update the reservation
        var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
        await _reservationCollection.ReplaceOneAsync(filter, existingReservation);

        return "Reservation updated successfully";
    }

    public async Task<string> DeleteAsync(string id)
    {
        // Check if the reservation exists
        var reservation = await _reservationCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        if (reservation == null)
            return "Reservation not found";

        // Check if it's at least 5 days before the reservation date
        if ((reservation.ReservedOn - DateTime.Now).Days < 5)
            return "Cannot delete reservation within 5 days of the reservation date";

        // Delete the reservation
        await _reservationCollection.DeleteOneAsync(r => r.Id == id);

        // Remove reservation Id from Train and User collections
        await RemoveReservationIdsFromTrainAndUserCollections(reservation.TrainId, reservation.UserNIC, id);

        return "Reservation deleted successfully";
    }

    // Additional methods for updating and removing reservation Ids from Train and User collections
    private async Task UpdateTrainAndUserCollections(string trainId, string userNIC, string reservationId)
    {
        var filterTrain = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var updateTrain = Builders<Train>.Update.Push(t => t.Reservations, reservationId);
        await _trainCollection.UpdateOneAsync(filterTrain, updateTrain);

        var filterUser = Builders<User>.Filter.Eq(u => u.NIC, userNIC);
        var updateUser = Builders<User>.Update.Push(u => u.ReservationIds, reservationId);
        await _userCollection.UpdateOneAsync(filterUser, updateUser);
    }

    private async Task RemoveReservationIdsFromTrainAndUserCollections(string trainId, string userNIC, string reservationId)
    {
        var filterTrain = Builders<Train>.Filter.Eq(t => t.Id, trainId);
        var updateTrain = Builders<Train>.Update.Pull(t => t.Reservations, reservationId);
        await _trainCollection.UpdateOneAsync(filterTrain, updateTrain);

        var filterUser = Builders<User>.Filter.Eq(u => u.NIC, userNIC);
        var updateUser = Builders<User>.Update.Pull(u => u.ReservationIds, reservationId);
        await _userCollection.UpdateOneAsync(filterUser, updateUser);
    }
}
