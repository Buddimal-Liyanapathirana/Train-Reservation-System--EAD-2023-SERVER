using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDotnetDemo.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


public class UserService : IUserService
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IReservationService _reservationService;
    private readonly IConfiguration _configuration;


    public UserService(IOptions<DatabaseSettings> dbSettings , IReservationService reservationService, IConfiguration configuration)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Value.UsersCollectionName);
        _reservationService = reservationService;
        _configuration = configuration;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        //get all users
        var users = await _userCollection.Find(_ => true).ToListAsync();
        return users;
    }

    public async Task<User> GetByIdAsync(string nic)
    {
        //get users by id
        var user = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        return user;
    }

    public async Task<IEnumerable<User>> GetUsersForActivation()
    {
        //get users that equested activation
        var users = await _userCollection.Find(u => u.isActivationPending == true).ToListAsync();
        return users;
    }

    public async Task<string> Login(string nic, string password)
    {
        //login for user
        var existingUser = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();

        if (existingUser == null || !BCrypt.Net.BCrypt.Verify(password, existingUser.PasswordHash))
        {
            return "Invalid email or password";
        }

        string token = GenerateToken(existingUser.NIC, existingUser.Role);
        return token  ;
    }

    public async Task<string> CreateAsync(User user)
    {
        //creates user
        user.IsActive = true;
        user.ReservationIds = new List<string>();
        var nicRegex = "^[0-9]{12}$|^[0-9]{9}v$";
        if (user.NIC == null || !Regex.IsMatch(user.NIC, nicRegex))
        {
            return "Invalid NIC";
        }

        user.isActivationPending = false;    
        user.PasswordHash = EncryptPassword(user.PasswordHash);
        await _userCollection.InsertOneAsync(user);
        return "User created successfully";
    }

    public async Task<string> UpdateAsync(string nic, User newUser)
    {
        //updates user
        var existingUser = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        if (existingUser == null)
            return "User not found";

        newUser.ReservationIds = existingUser.ReservationIds;
        newUser.IsActive = existingUser.IsActive;

        var filter = Builders<User>.Filter.Eq(u => u.NIC, nic);
        var update = Builders<User>.Update
            .Set(u => u.UserName, newUser.UserName)
            .Set(u => u.PasswordHash, EncryptPassword(newUser.PasswordHash))
            .Set(u => u.Email, newUser.Email)
            .Set(u => u.Role, newUser.Role);

        await _userCollection.UpdateOneAsync(filter, update);
        return "User updated successfully";
    }

    public async Task<string> ActivateUserAsync(string nic)
    {
        //set user isctive status
        var user = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        if (user == null)
            return "User not found";

        user.IsActive = true;
        user.ReservationIds = new List<string>();
        user.isActivationPending = false;

        await _userCollection.ReplaceOneAsync(u => u.NIC == nic, user);
        return "User activated successfully";
    }

    public async Task<string> DeactivateUserAsync(string nic)
    {
        //deactvate a user
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

    public async Task<string> RequestActivation(string nic)
    {
        //request for activation by inactive users
        var user = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();

        if (user == null)
            return "User not found";
        if (user.IsActive == true)
            return "User is already active";

        user.isActivationPending = true;

        var filter = Builders<User>.Filter.Eq(u => u.NIC, nic);
        var update = Builders<User>.Update
            .Set(u => u.isActivationPending, true);
        await _userCollection.UpdateOneAsync(filter, update);

        return "Activaton request placed successfully";
    }

    public async Task<string> DeleteAsync(string nic)
    {
        //delete user
        var user = await _userCollection.Find(u => u.NIC == nic).FirstOrDefaultAsync();
        if (user == null)
            return "User not found";

        if (user.IsActive == true)
            return "Cannot delete an active user";

        await _userCollection.DeleteOneAsync(u => u.NIC == nic);
        return "User deleted successfully";
    }

    private static string EncryptPassword(string password)
    {
        //encrypt user password
        // Generate a salt 
        string salt = BCrypt.Net.BCrypt.GenerateSalt();

        // Hash the password 
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return hashedPassword;
    }

    private string GenerateToken(string id, string role)
    {
        //generate jwt token for authorization
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, id),
            new Claim(ClaimTypes.Role, role)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("JwtSettings:SecretKey").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

}
