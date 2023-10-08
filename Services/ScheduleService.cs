using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDotnetDemo.Models;

public class ScheduleService : IScheduleService
{
    private readonly IMongoCollection<Schedule> _scheduleCollection;
    private readonly IOptions<DatabaseSettings> _dbSettings;

    public ScheduleService(IOptions<DatabaseSettings> dbSettings)
    {
        _dbSettings = dbSettings;
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _scheduleCollection = mongoDatabase.GetCollection<Schedule>(dbSettings.Value.SchedulesCollectionName);
    }

    public async Task<IEnumerable<Schedule>> GetAllAsync()
    {
        var schedules = await _scheduleCollection.Find(_ => true).ToListAsync();
        return schedules;
    }

    public async Task<Schedule> GetByIdAsync(string id)
    {
        var schedule = await _scheduleCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
        return schedule;
    }

    public async Task CreateAsync(Schedule schedule)
    {
        await _scheduleCollection.InsertOneAsync(schedule);
    }

    public async Task UpdateAsync(string id, Schedule schedule)
    {
        var filter = Builders<Schedule>.Filter.Eq(s => s.Id, id);
        var update = Builders<Schedule>.Update
            .Set(s => s.DepartureStation, schedule.DepartureStation)
            .Set(s => s.ArrivalStation, schedule.ArrivalStation)
            .Set(s => s.DepartureTime, schedule.DepartureTime)
            .Set(s => s.ArrivalTime, schedule.ArrivalTime)
            .Set(s => s.OperatingDays, schedule.OperatingDays);

        await _scheduleCollection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAsync(string id)
    {
        await _scheduleCollection.DeleteOneAsync(s => s.Id == id);
    }
}
