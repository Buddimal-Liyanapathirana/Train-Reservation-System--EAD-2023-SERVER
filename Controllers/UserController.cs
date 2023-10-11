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
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(string nic , string password)
    {
        string result = await _userService.Login(nic, password);

        if (result.Contains("Invalid"))
            return BadRequest(new ApiResponse<string>(true, result, null));

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post(User user)
    {
        var result = await _userService.CreateAsync(user);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }   
    
    
    [HttpPut("requestActivation")]
    public async Task<IActionResult> RequestActivation(string nic)
    {
        var result = await _userService.RequestActivation(nic);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpPut("{nic}")]
    public async Task<IActionResult> Put(string nic, [FromBody] User newUser)
    {
        var result = await _userService.UpdateAsync(nic, newUser);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpPut("activate/{nic}")]
    public async Task<IActionResult> ActivateUser(string nic)
    {
        var result = await _userService.ActivateUserAsync(nic);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpPut("deactivate/{nic}")]
    public async Task<IActionResult> DeactivateUser(string nic)
    {
        var result = await _userService.DeactivateUserAsync(nic);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null));
    }

    [HttpDelete("{nic}")]
    public async Task<IActionResult> Delete(string nic)
    {
        var result = await _userService.DeleteAsync(nic);
        if (result.Contains("successfully"))
            return Ok(new ApiResponse<string>(true, result, null));

        return BadRequest(new ApiResponse<string>(false, result, null)); 
    }
}
