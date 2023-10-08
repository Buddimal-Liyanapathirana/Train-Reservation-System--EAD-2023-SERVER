using MongoDotnetDemo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IScheduleService
{
    Task<IEnumerable<Schedule>> GetAllAsync();
    Task<Schedule> GetByIdAsync(string id);
    Task CreateAsync(Schedule schedule);
    Task UpdateAsync(string id, Schedule schedule);
    Task DeleteAsync(string id);
}
