using MongoDB.Bson;
using MongoDotnetDemo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> GetByIdAsync(string nic);
    Task<string> CreateAsync(User user);
    Task<string> UpdateAsync(string nic, User user);
    Task<string> ActivateUserAsync(string nic);
    Task<string> DeactivateUserAsync(string nic);
    Task<string> DeleteAsync(string nic);
    Task<string> Login(string nic, string password);
    Task<IEnumerable<User>> GetUsersForActivation();
    Task<string> RequestActivation(string nic);
}
