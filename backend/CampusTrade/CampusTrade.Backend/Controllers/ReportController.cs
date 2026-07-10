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
    private readonly IAdminService _adminService;

    public ReportController(IReportService reportService, IAuthService authService, IAdminService adminService)
    {
        _reportService = reportService;
        _authService = authService;
        _adminService = adminService;
    }

    /// <summary>获取举报列表（分页、可筛选类型和状态）</summary>
    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? reportType = null,
        [FromQuery] string? status = null)
    {
        try
        {
            await _adminService.RequireAdminAsync(ResolveCurrentUserId());
            var result = await _reportService.GetReportsAsync(page, size, reportType, status);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    /// <summary>获取单条举报详情</summary>
    [HttpGet("{reportId:int}")]
    public async Task<IActionResult> GetReport(int reportId)
    {
        try
        {
            var result = await _reportService.GetReportByIdAsync(reportId);
            if (result.Code == 404) return NotFound(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    /// <summary>提交举报</summary>
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            if (!currentUserId.HasValue)
                return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));

            var result = await _reportService.CreateReportAsync(request, currentUserId.Value);
            if (result.Code != 200) return StatusCode(result.Code, result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    /// <summary>处理举报（仅管理员）</summary>
    [HttpPut("{reportId:int}/handle")]
    public async Task<IActionResult> HandleReport(int reportId, [FromBody] HandleReportRequest request)
    {
        try
        {
            var adminId = await _adminService.RequireAdminAsync(ResolveCurrentUserId());
            var result = await _reportService.HandleReportAsync(reportId, request, adminId);
            if (result.Code != 200) return StatusCode(result.Code, result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
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
            return null;

        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : value.Trim();
    }

    private IActionResult ToErrorResult(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException uae when uae.Message.Contains("admin", StringComparison.OrdinalIgnoreCase) =>
                StatusCode(403, ApiResponse<object>.Fail(403, uae.Message)),
            UnauthorizedAccessException uae =>
                Unauthorized(ApiResponse<object>.Fail(401, uae.Message)),
            ArgumentException ae =>
                BadRequest(ApiResponse<object>.Fail(400, ae.Message)),
            InvalidOperationException ioe when ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) =>
                NotFound(ApiResponse<object>.Fail(404, ioe.Message)),
            InvalidOperationException ioe =>
                BadRequest(ApiResponse<object>.Fail(400, ioe.Message)),
            _ => StatusCode(500, ApiResponse<object>.Fail(500, "internal server error"))
        };
    }
}
