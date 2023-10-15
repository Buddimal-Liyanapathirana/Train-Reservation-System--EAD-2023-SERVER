using Microsoft.AspNetCore.Mvc;
using MongoDotnetDemo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class RouteController : ControllerBase
{
    private readonly IRouteService _routeService;

    public RouteController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var routes = await _routeService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<Route>>(true, "Routes retrieved successfully", routes));
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new ApiResponse<object>(false, "An error occurred while processing the request", null));
        }
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> Get(string name)
    {
        try
        {
            var route = await _routeService.GetByNameAsync(name);
            if (route == null)
            {
                return NotFound(new ApiResponse<object>(false, "Route not found", null));
            }
            return Ok(new ApiResponse<Route>(true, "Route retrieved successfully", route));
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new ApiResponse<object>(false, "An error occurred while processing the request", null));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post(Route route)
    {
        try
        {
            string result =  await _routeService.CreateAsync(route);
            return Ok(new ApiResponse<string>(true,result, null));
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new ApiResponse<object>(false, "An error occurred while processing the request", null));
        }
    }

    [HttpPut("{name}")]
    public async Task<IActionResult> Put(string name, [FromBody] Route newRoute)
    {
        try
        {
            var route = await _routeService.GetByNameAsync(name);
            if (route == null)
                return NotFound(new ApiResponse<object>(false, "Route not found", null));

            await _routeService.UpdateAsync(name, newRoute);
            return Ok(new ApiResponse<string>(true, "Route updated successfully", null));
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new ApiResponse<object>(false, "An error occurred while processing the request", null));
        }
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name)
    {
        try
        {
            var route = await _routeService.GetByNameAsync(name);
            if (route == null)
                return NotFound(new ApiResponse<object>(false, "Route not found", null));

            var result =await _routeService.DeleteAsync(name);
            return Ok(new ApiResponse<string>(true, result, null));
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new ApiResponse<object>(false, "An error occurred while processing the request", null));
        }
    }
}
