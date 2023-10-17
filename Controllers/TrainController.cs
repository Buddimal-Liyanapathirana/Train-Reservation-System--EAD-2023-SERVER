using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDotnetDemo.Models;
using System.Threading.Tasks;
using TrainReservationSystem.DTO;

[Route("api/[controller]")]
[ApiController]
public class TrainController : ControllerBase
{
    private readonly ITrainService _trainService;

    public TrainController(ITrainService trainService)
    {
        _trainService = trainService;
    }


    [HttpGet]
    public async Task<IActionResult> Get()
    {
        //get all trains
        var trains = await _trainService.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<Train>>(true, "Trains retrieved successfully", trains));
    }

    [HttpGet("/active")]
    public async Task<IActionResult> GetActiveTrains()
    {
        //get all active trains
        var trains = await _trainService.GetActiveTrains();
        return Ok(new ApiResponse<IEnumerable<ActiveTrainsForBooking>>(true, "Trains retrieved successfully", trains));
    }


    [HttpPost("/active/withroute")]
    public async Task<IActionResult> GetActiveTrainsForRoute([FromBody]string route)
    {
        //get all active trains
        var trains = await _trainService.GetActiveTrainsForRoute(route);
        return Ok(new ApiResponse<IEnumerable<ActiveTrainsForBooking>>(true, "Trains retrieved successfully", trains));
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        //get train by id
        var train = await _trainService.GetByIdAsync(id);
        if (train == null)
        {
            return NotFound(new ApiResponse<string>(false, "Train not found", null));
        }
        return Ok(new ApiResponse<Train>(true, "Train retrieved successfully", train));
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TrainDTO createTrainDTO)
    {
        //create train
        Train newTrain = new Train(createTrainDTO.trainName, createTrainDTO.luxurySeatCount, createTrainDTO.economySeatCount);
        var result = await _trainService.CreateAsync(newTrain);
        return Ok(new ApiResponse<string>(true, result, null));
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] TrainDTO createTrainDTO)
    {
        //update train
        Train newTrain = new Train(createTrainDTO.trainName, createTrainDTO.luxurySeatCount, createTrainDTO.economySeatCount);
        var result = await _trainService.UpdateAsync(id, newTrain);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpPut("add-schedule/{id}/{scheduleId}")]
    public async Task<IActionResult> AddSchedule(string id, string scheduleId)
    {
        //assign schedule to train
        var result = await _trainService.AddScheduleAsync(id, scheduleId);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }


    [HttpPut("activate/{id}")]
    public async Task<IActionResult> ActivateTrain(string id)
    {
        //activate train
        var result = await _trainService.ActivateTrainAsync(id);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }


    [HttpPut("deactivate/{id}")]
    public async Task<IActionResult> DeactivateTrain(string id)
    {
        //deactivate train
        var result = await _trainService.DeactivateTrainAsync(id);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        //delete train
        var result = await _trainService.DeleteAsync(id);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));
        
        return BadRequest(new ApiResponse<string>(false, result, null));    
    }

}
