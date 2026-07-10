using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IAuthService _authService;

    public ReportController(IReportService reportService, IAuthService authService)
    {
        _reportService = reportService;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReports([FromQuery] int page = 1, [FromQuery] int size = 20, [FromQuery] string? reportType = null, [FromQuery] string? status = null)
    {
        var result = await _reportService.GetReportsAsync(page, size, reportType, status);
        return Ok(result);
    }

    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetReport(int reportId)
    {
        var result = await _reportService.GetReportByIdAsync(reportId);
        if (result.Code == 404) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        var currentUserId = ResolveCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));

        var result = await _reportService.CreateReportAsync(request, currentUserId.Value);
        if (result.Code != 200) return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpPut("{reportId}/handle")]
    public async Task<IActionResult> HandleReport(int reportId, [FromBody] HandleReportRequest request)
    {
        var adminId = ResolveCurrentUserId();
        if (!adminId.HasValue)
            return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));

        var result = await _reportService.HandleReportAsync(reportId, request, adminId.Value);
        if (result.Code != 200) return StatusCode(result.Code, result);
        return Ok(result);
    }

    private int? ResolveCurrentUserId()
    {
        var token = ReadBearerToken();
        return _authService.TryGetUserIdFromToken(token);
    }

    private string? ReadBearerToken()
    {
        var value = Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : value.Trim();
    }
}