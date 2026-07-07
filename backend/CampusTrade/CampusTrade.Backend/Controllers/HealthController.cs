using CampusTrade.Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var data = new
        {
            status = "backend is running"
        };

        return Ok(ApiResponse<object>.Success(data));
    }
}