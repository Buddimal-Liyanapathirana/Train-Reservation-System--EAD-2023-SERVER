using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDotnetDemo.Models;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IReservationService _reservationService;


    public UserService(IOptions<DatabaseSettings> dbSettings , IReservationService reservationService)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Value.UsersCollectionName);
        _reservationService = reservationService;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = await _userCollection.Find(_ => true).ToListAsync();
        return users;
    }

    public async Task<User> GetByIdAsync(string nic)
    {
        var user = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        return user;
    }

    public async Task<string> CreateAsync(User user)
    {
        user.IsActive = true;
        user.ReservationIds = new List<string>();
        if (user.NIC == null || user.NIC =="") {
            return "Invalid NIC";
        }

        await _userCollection.InsertOneAsync(user);
        return "User created successfully";
    }

    public async Task<string> UpdateAsync(string nic, User newUser)
    {
        var existingUser = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        if (existingUser == null)
            return "User not found";

        newUser.ReservationIds = existingUser.ReservationIds;
        newUser.IsActive = existingUser.IsActive;

        var filter = Builders<User>.Filter.Eq(u => u.NIC, nic);
        var update = Builders<User>.Update
            .Set(u => u.UserName, newUser.UserName)
            .Set(u => u.PasswordHash, newUser.PasswordHash)
            .Set(u => u.Role, newUser.Role);

        await _userCollection.UpdateOneAsync(filter, update);
        return "User updated successfully";
    }

    public async Task<string> ActivateUserAsync(string nic)
    {
        var user = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        if (user == null)
            return "User not found";

        user.IsActive = true;
        user.ReservationIds = new List<string>();

        await _userCollection.ReplaceOneAsync(u => u.NIC == nic, user);
        return "User activated successfully";
    }

    public async Task<string> DeactivateUserAsync(string nic)
    {
        var user = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        if (user == null)
            return "User not found";

        if (user.ReservationIds != null)
        {
            foreach (var reservationId in user.ReservationIds)
            {
                await _reservationService.DeleteAsync(reservationId);
            }
        }

        user.IsActive = false;
        user.ReservationIds = new List<string>();

        await _userCollection.ReplaceOneAsync(u => u.NIC == nic, user);
        return "User deactivated successfully";
    }

    public async Task<string> DeleteAsync(string nic)
    {
        var user = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        if (user == null)
            return "User not found";

        if (user.IsActive == true)
            return "Cannot delete an active user";

        await _userCollection.DeleteOneAsync(u => u.NIC == nic);
        return "User deleted successfully";
    }

}
