using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAuthService _authService;

    public AdminController(IAdminService adminService, IAuthService authService)
    {
        _adminService = adminService;
        _authService = authService;
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] int? adminId = null,
        [FromQuery] string? auditType = null)
    {
        try
        {
            var result = await _adminService.GetAuditLogsAsync(ResolveCurrentUserId(), page, size, adminId, auditType);
            return Ok(ApiResponse<AuditLogListResult>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpGet("audit-logs/{logId:int}")]
    public async Task<IActionResult> GetAuditLogById(int logId)
    {
        try
        {
            var log = await _adminService.GetAuditLogByIdAsync(ResolveCurrentUserId(), logId);
            if (log == null)
            {
                return NotFound(ApiResponse<object>.Fail(404, "audit log not found"));
            }

            return Ok(ApiResponse<AuditLogDto>.Success(log));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPost("audit-logs")]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
    {
        try
        {
            var log = await _adminService.CreateAuditLogAsync(ResolveCurrentUserId(), request);
            return Ok(ApiResponse<AuditLogDto>.Success(log, "audit log created"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpGet("notices")]
    public async Task<IActionResult> GetNotices(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? noticeType = null)
    {
        try
        {
            var result = await _adminService.GetNoticesAsync(ResolveCurrentUserId(), page, size, noticeType);
            return Ok(ApiResponse<NoticeListResult>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpGet("notices/{noticeId:int}")]
    public async Task<IActionResult> GetNoticeById(int noticeId)
    {
        try
        {
            var notice = await _adminService.GetNoticeByIdAsync(ResolveCurrentUserId(), noticeId);
            if (notice == null)
            {
                return NotFound(ApiResponse<object>.Fail(404, "notice not found"));
            }

            return Ok(ApiResponse<NoticeDto>.Success(notice));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPost("notices")]
    public async Task<IActionResult> CreateNotice([FromBody] CreateNoticeRequest request)
    {
        try
        {
            var notice = await _adminService.CreateNoticeAsync(ResolveCurrentUserId(), request);
            return Ok(ApiResponse<NoticeDto>.Success(notice, "notice created"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("notices/{noticeId:int}")]
    public async Task<IActionResult> UpdateNotice(int noticeId, [FromBody] UpdateNoticeRequest request)
    {
        try
        {
            var notice = await _adminService.UpdateNoticeAsync(ResolveCurrentUserId(), noticeId, request);
            return Ok(ApiResponse<NoticeDto>.Success(notice, "notice updated"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpDelete("notices/{noticeId:int}")]
    public async Task<IActionResult> DeleteNotice(int noticeId)
    {
        try
        {
            var deleted = await _adminService.DeleteNoticeAsync(ResolveCurrentUserId(), noticeId);
            if (!deleted)
            {
                return NotFound(ApiResponse<object>.Fail(404, "notice not found"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "notice deleted"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    private int? ResolveCurrentUserId()
    {
        return _authService.TryGetUserIdFromToken(ReadBearerToken());
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

    private IActionResult ToErrorResult(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException uae when uae.Message.Contains("admin", StringComparison.OrdinalIgnoreCase) =>
                StatusCode(403, ApiResponse<object>.Fail(403, uae.Message)),
            UnauthorizedAccessException uae => Unauthorized(ApiResponse<object>.Fail(401, uae.Message)),
            ArgumentException ae => BadRequest(ApiResponse<object>.Fail(400, ae.Message)),
            InvalidOperationException ioe when ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) =>
                NotFound(ApiResponse<object>.Fail(404, ioe.Message)),
            InvalidOperationException ioe => BadRequest(ApiResponse<object>.Fail(400, ioe.Message)),
            _ => StatusCode(500, ApiResponse<object>.Fail(500, "internal server error"))
        };
    }
}