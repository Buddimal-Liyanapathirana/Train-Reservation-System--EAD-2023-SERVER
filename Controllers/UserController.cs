using Microsoft.AspNetCore.Mvc;
using MongoDotnetDemo.Models;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var users = await _userService.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<User>>(true, "User retrieved successfully", users));
    }

    [HttpGet("{nic}")]
    public async Task<IActionResult> Get(string nic)
    {
        var user = await _userService.GetByIdAsync(nic);
        if (user == null)
        {
            return NotFound(new ApiResponse<string>(false, "User not found", null));
        }
        return Ok(new ApiResponse<User>(true, "User retrieved successfully", user));
    }

    [HttpPost]
    public async Task<IActionResult> Post(User user)
    {
        var result = await _userService.CreateAsync(user);
        return Ok(new ApiResponse<string>(true, "User created successfully", null));
    }

    [HttpPut("{nic}")]
    public async Task<IActionResult> Put(string nic, [FromBody] User newUser)
    {
        var result = await _userService.UpdateAsync(nic, newUser);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpPut("activate/{nic}")]
    public async Task<IActionResult> ActivateUser(string nic)
    {
        var result = await _userService.ActivateUserAsync(nic);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpPut("deactivate/{nic}")]
    public async Task<IActionResult> DeactivateUser(string nic)
    {
        var result = await _userService.DeactivateUserAsync(nic);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }

    [HttpDelete("{nic}")]
    public async Task<IActionResult> Delete(string nic)
    {
        var result = await _userService.DeleteAsync(nic);
        if (result.Contains("not found"))
            return NotFound(new ApiResponse<string>(false, result, null));
        if (result.Contains("active"))
            return BadRequest(new ApiResponse<string>(false, result, null));
        return Ok(new ApiResponse<string>(true, result, null));
    }
}
