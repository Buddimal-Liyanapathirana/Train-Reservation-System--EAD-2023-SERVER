﻿using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDotnetDemo.Models;

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
        var trains = await _trainCollection.Find(_ => true).ToListAsync();
        return trains;
    }

    public async Task<Train> GetByIdAsync(string id)
    {
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        return train;
    }

    public async Task<string> CreateAsync(Train train)
    {
        train.IsActive = true;
        train.Reservations = new List<string>();
        train.Schedule = null;

        await _trainCollection.InsertOneAsync(train);
        return "Train created successfully";
    }

    public async Task<string> UpdateAsync(string id, Train newTrain)
    {
        var existingTrain = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (existingTrain == null)
            return "Train not found";

        // Do not allow altering Reservations, IsActive, and Schedule
        newTrain.Reservations = existingTrain.Reservations;
        newTrain.IsActive = existingTrain.IsActive;
        newTrain.Schedule = existingTrain.Schedule;

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
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        var schedule = await _scheduleCollection.Find(s => s.Id == scheduleId).FirstOrDefaultAsync();
        if (schedule == null)
            return "Schedule not found";

        train.Schedule = scheduleId;
        await _trainCollection.ReplaceOneAsync(t => t.Id == id, train);
        return "Schedule added to the train successfully";
    }

    public async Task<string> RemoveScheduleAsync(string id)
    {
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        if (train.Reservations != null && train.Reservations.Count > 0)
            return "Cannot remove schedule when there are reservations";

        train.Schedule = null;
        await _trainCollection.ReplaceOneAsync(t => t.Id == id, train);
        return "Schedule removed from the train successfully";
    }

    public async Task<string> ActivateTrainAsync(string id)
    {
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        train.IsActive = true;
        await _trainCollection.ReplaceOneAsync(t => t.Id == id, train);
        return "Train activated successfully";
    }

    public async Task<string> DeactivateTrainAsync(string id)
    {
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        if (train.Reservations != null && train.Reservations.Count > 0)
            return "Cannot deactivate train with reservations";

        // Remove schedule
        train.IsActive = false;

        await _trainCollection.ReplaceOneAsync(t => t.Id == id, train);
        return "Train deactivated successfully";
    }

    public async Task<string> DeleteAsync(string id)
    {
        var train = await _trainCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (train == null)
            return "Train not found";

        if (train.IsActive == true)
            return "Cannot delete an active train";

        await _trainCollection.DeleteOneAsync(t => t.Id == id);
        return "Train deleted successfully";
    }
}
