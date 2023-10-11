﻿using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITrainService
{
    Task<IEnumerable<Train>> GetAllAsync();
    //Task<IEnumerable<Train>> GetTrainsToReserve(DateTime date,string route , string from , string to);
    Task<Train> GetByIdAsync(string id);
    Task<string> CreateAsync(Train train);
    Task<string> UpdateAsync(string id, Train train);
    Task<string> AddScheduleAsync(string id, string scheduleId);
    Task<string> ActivateTrainAsync(string id);
    Task<string> DeactivateTrainAsync(string id);
    Task<string> DeleteAsync(string id);
}
