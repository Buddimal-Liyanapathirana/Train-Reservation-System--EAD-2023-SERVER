using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainReservationSystem.DTO;

public interface ITrainService
{
    Task<IEnumerable<Train>> GetAllAsync();
    Task<IEnumerable<ActiveTrainsForBooking>> GetActiveTrains();
    Task<Train> GetByIdAsync(string id);
    Task<string> CreateAsync(Train train);
    Task<string> UpdateAsync(string id, Train train);
    Task<string> AddScheduleAsync(string id, string scheduleId);
    Task<string> ActivateTrainAsync(string id);
    Task<string> DeactivateTrainAsync(string id);
    Task<string> DeleteAsync(string id);
}
