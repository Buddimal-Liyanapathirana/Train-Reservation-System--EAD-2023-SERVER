using Microsoft.AspNetCore.Mvc;
using MongoDotnetDemo.Models;
using System.Threading.Tasks;
using TrainReservationSystem.DTO;

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
        //get all reservations
        var reservations = await _reservationService.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable< Reservation>>(true, "Reservations retrieved successfully", reservations));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        //get reservations by id
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation == null)
            return NotFound(new ApiResponse<string>(false, "Reservation not found", null));

        return Ok(new ApiResponse<Reservation>(true, "Reservations retrieved successfully", reservation));
    }

    [HttpGet("user/{userNic}")]
    public async Task<IActionResult> GetByUserNicAsync(string userNic)
    {
        //get reservations by user NIC
        if(userNic.Length > 12)
            return BadRequest(new ApiResponse<string>(false, "Invalid user id", null));

        var reservation = await _reservationService.GetByUserNicAsync(userNic);
        if (reservation == null)
            return NotFound(new ApiResponse<string>(false, "Reservations not found", null));
        
        return Ok(new ApiResponse<IEnumerable<Reservation>>(true, "Reservation retrieved successfully", reservation));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]ReservationDTO reservationDTO)
    {
        //create reservation
        Reservation reservation = new Reservation();
        reservation.LuxurySeats = reservationDTO.luxurySeats;
        reservation.EconomySeats = reservationDTO.economySeats;
        reservation.startStation = reservationDTO.startStation;
        reservation.endStation = reservationDTO.endStation;
        reservation.UserNIC = reservationDTO.userNIC;
        reservation.TrainId = reservationDTO.trainId;
        reservation.ReservedOn = reservationDTO.reservationDate;

        var result = await _reservationService.CreateAsync(reservation);
        if(result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));
     
        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpPost("/request")]
    public async Task<IActionResult> PostResRequest([FromBody]ReservationRequestDTO reservationRequestDTO)
    {
        //create reservation
        Reservation reservation = new Reservation();
        reservation.LuxurySeats = reservationRequestDTO.luxurySeats;
        reservation.EconomySeats = reservationRequestDTO.economySeats;
        reservation.RequestedBy = reservationRequestDTO.resuestedUserId;
        reservation.RequestedToDate = reservationRequestDTO.requestForDate;
        reservation.startStation = reservationRequestDTO.startStation;
        reservation.endStation = reservationRequestDTO.endStation;
        reservation.isRequested = true;

        var result = await _reservationService.CreateRequestAsync(reservation);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody]EditReservationDTO editReservationDTO)
    {
        //update reservation
        Reservation reservation = new Reservation();
        reservation.LuxurySeats = editReservationDTO.luxurySeats;
        reservation.EconomySeats = editReservationDTO.economySeats;
        reservation.startStation = editReservationDTO.startStation;
        reservation.endStation = editReservationDTO.endStation;
        reservation.ReservedOn = editReservationDTO.reservationDate;

        var result = await _reservationService.UpdateAsync(id, reservation);
            if(result.Contains("successfully"))
                return Ok(new ApiResponse<string>(true, result, null));

            return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        //delete reservation
        var result = await _reservationService.DeleteAsync(id);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpPut("complete/{id}")]
    public async Task<IActionResult> CompleteReservation(string id)
    {
        //marks reservation as complete . this is displayed in reservation history
        var result = await _reservationService.CompleteReservation(id);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }
}
