using System.Collections.Generic;
using System.Threading.Tasks;

public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<Reservation> GetByIdAsync(string id);
    Task<string> CreateAsync(Reservation reservation);
    Task<string> UpdateAsync(string id, Reservation reservation);
    Task<string> DeleteAsync(string id);
}
