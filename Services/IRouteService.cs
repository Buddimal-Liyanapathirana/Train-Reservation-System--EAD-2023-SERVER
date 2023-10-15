using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRouteService
{
    Task<IEnumerable<Route>> GetAllAsync();
    Task<Route> GetByNameAsync(string name);
    Task<string> CreateAsync(Route route);
    Task<string> UpdateAsync(string name, Route route);
    Task<string> DeleteAsync(string name);
}
