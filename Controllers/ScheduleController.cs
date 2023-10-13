using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDotnetDemo.Models;
using System.Threading.Tasks;

namespace MongoDotnetDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //get all schedules
            var schedules = await _scheduleService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<Schedule>>(true, "Schedules retrieved successfully", schedules));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            //get schedule by id
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(new ApiResponse<string>(false, "Schedule not found", null));
            }
            return Ok(new ApiResponse<Schedule>(true, "Schedule retrieved successfully", schedule));
        }

        [HttpPost]
        public async Task<IActionResult> Post(Schedule schedule)
        {
            //create schedule
            var result = await _scheduleService.CreateAsync(schedule);
            return Ok(new ApiResponse<string>(true, result, null));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Schedule newSchedule)
        {      
            //update schdeule
            var result =  await _scheduleService.UpdateAsync(id, newSchedule);
            if (result.Contains("successfully"))
                return Ok(new ApiResponse<string>(true, result, null));

            return BadRequest(new ApiResponse<string>(false, result, null));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //delete schedule
            var result = await _scheduleService.DeleteAsync(id);
            if (result.Contains("successfully"))
                return Ok(new ApiResponse<string>(true, result, null));

            return BadRequest(new ApiResponse<string>(false, result, null));
        }
    }
}
