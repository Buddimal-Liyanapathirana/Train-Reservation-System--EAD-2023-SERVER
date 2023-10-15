using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDotnetDemo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RouteService : IRouteService
{
    private readonly IMongoCollection<Route> _routeCollection;

    public RouteService(IOptions<DatabaseSettings> dbSettings)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _routeCollection = mongoDatabase.GetCollection<Route>(dbSettings.Value.RoutesCollectionName);
    }

    public async Task<IEnumerable<Route>> GetAllAsync()
    {
        var routes = await _routeCollection.Find(_ => true).ToListAsync();
        return routes;
    }

    public async Task<Route> GetByNameAsync(string name)
    {
        var route = await _routeCollection.Find(r => r.Name == name).FirstOrDefaultAsync();
        return route;
    }

    public async Task<string> CreateAsync(Route route)
    {
        if(route.Name == "")
            return "Invalid route name";

        await _routeCollection.InsertOneAsync(route);
        return "Route created successfully";
    }

    public async Task<string> UpdateAsync(string name, Route route)
    {
        var filter = Builders<Route>.Filter.Eq(r => r.Name, name);
        var update = Builders<Route>.Update
            .Set(r => r.Stations, route.Stations);

        await _routeCollection.UpdateOneAsync(filter, update);
        return "Route updated successfully";
    }

    public async Task<string> DeleteAsync(string name)
    {
        await _routeCollection.DeleteOneAsync(r => r.Name == name);
        return "Route deleted successfully";
    }
}
