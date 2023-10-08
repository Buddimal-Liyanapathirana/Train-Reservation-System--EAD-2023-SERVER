using Microsoft.AspNetCore.Mvc;
using MongoDotnetDemo.Models;
using System.Threading.Tasks;

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
        var trains = await _trainService.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<Train>>(true, "Trains retrieved successfully", trains));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var train = await _trainService.GetByIdAsync(id);
        if (train == null)
        {
            return NotFound(new ApiResponse<string>(false, "Train not found", null));
        }
        return Ok(new ApiResponse<Train>(true, "Train retrieved successfully", train));
    }

    [HttpPost]
    public async Task<IActionResult> Post(Train train)
    {
        var result = await _trainService.CreateAsync(train);
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Train newTrain)
    {
        var result = await _trainService.UpdateAsync(id, newTrain);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpPut("add-schedule/{id}/{scheduleId}")]
    public async Task<IActionResult> AddSchedule(string id, string scheduleId)
    {
        var result = await _trainService.AddScheduleAsync(id, scheduleId);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpPut("remove-schedule/{id}")]
    public async Task<IActionResult> RemoveSchedule(string id)
    {
        var result = await _trainService.RemoveScheduleAsync(id);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        if (result.Contains("reservations"))
            return BadRequest(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpPut("activate/{id}")]
    public async Task<IActionResult> ActivateTrain(string id)
    {
        var result = await _trainService.ActivateTrainAsync(id);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpPut("deactivate/{id}")]
    public async Task<IActionResult> DeactivateTrain(string id)
    {
        var result = await _trainService.DeactivateTrainAsync(id);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        if (result.Contains("reservations"))
            return BadRequest(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _trainService.DeleteAsync(id);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        if (result.Contains("active"))
            return BadRequest(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

}
