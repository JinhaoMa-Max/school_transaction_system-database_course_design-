using CampusSecondHand.Api.Common;
using CampusSecondHand.Api.Database;
using Microsoft.AspNetCore.Mvc;

namespace CampusSecondHand.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IOracleConnectionFactory _connectionFactory;

    public HealthController(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        return Ok(ApiResponse<object>.Ok(new { status = "ok" }, "API is running"));
    }

    [HttpGet("db")]
    public async Task<ActionResult<ApiResponse<object>>> GetDatabase()
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM dual";

        await command.ExecuteScalarAsync();

        return Ok(ApiResponse<object>.Ok(
            new
            {
                database = "oracle",
                status = "ok"
            },
            "Database connection is ok"));
    }
}
