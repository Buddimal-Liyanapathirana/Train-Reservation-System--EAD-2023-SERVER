using Microsoft.AspNetCore.Mvc;
using MongoDotnetDemo.Models;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var reservations = await _reservationService.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable< Reservation>>(true, "Reservations retrieved successfully", reservations));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation == null)
        {
            return NotFound(new ApiResponse<string>(false, "Reservation not found", null));
        }
        return Ok(new ApiResponse<Reservation>(true, "Reservation retrieved successfully", reservation));
    }

    [HttpPost]
    public async Task<IActionResult> Post(Reservation reservation)
    {
        var result = await _reservationService.CreateAsync(reservation);
        if (result.Contains("Invalid UserNIC or TrainId"))
            return BadRequest(new ApiResponse<string>(false, result, null));
        if (result.Contains("User has reached the maximum limit of reservations"))
            return BadRequest(new ApiResponse<string>(false, result, null));

        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, Reservation reservation)
    {
        var result = await _reservationService.UpdateAsync(id, reservation);
        if (result.Contains("Reservation not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        if (result.Contains("Cannot update reservation within 5 days of the reservation date"))
            return BadRequest(new ApiResponse<string>(false, result, null));

        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _reservationService.DeleteAsync(id);
        if (result.Contains("Reservation not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        if (result.Contains("Cannot delete reservation within 5 days of the reservation date"))
            return BadRequest(new ApiResponse<string>(false, result, null));

        return Ok(new ApiResponse<string>(true, result, null));
    }
}
