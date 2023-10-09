using MongoDotnetDemo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IScheduleService
{
    Task<IEnumerable<Schedule>> GetAllAsync();
    Task<Schedule> GetByIdAsync(string id);
    Task<string> CreateAsync(Schedule schedule);
    Task<string> UpdateAsync(string id, Schedule schedule);
    Task<string> DeleteAsync(string id);
    Task<bool> IsScheduleNotInUse(string id);
}
