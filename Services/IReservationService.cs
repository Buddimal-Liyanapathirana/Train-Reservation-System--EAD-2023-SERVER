using System.Collections.Generic;
using System.Threading.Tasks;

public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<IEnumerable<Reservation>> GetByUserNicAsync(string id);
    Task<Reservation> GetByIdAsync(string id);
    Task<string> CreateAsync(Reservation reservation);
    Task<string> UpdateAsync(string id, Reservation reservation);
    Task<string> DeleteAsync(string id);
    Task<string> CompleteReservation(string id);
}
