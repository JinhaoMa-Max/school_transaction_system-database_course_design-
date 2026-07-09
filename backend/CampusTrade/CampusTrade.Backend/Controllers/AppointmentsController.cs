using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/appointments")]
// 面交预约接口：对齐前端 appointment.ts
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAuthService _authService;

    public AppointmentsController(IAppointmentService appointmentService, IAuthService authService)
    {
        _appointmentService = appointmentService;
        _authService = authService;
    }

    // GET /api/appointments/order/{orderId}
    [HttpGet("order/{orderId:int}")]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _appointmentService.GetByOrderIdAsync(orderId, currentUserId);
            if (dto == null)
                return NotFound(ApiResponse<object>.Fail(404, "预约不存在"));
            return Ok(ApiResponse<AppointmentDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // POST /api/appointments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _appointmentService.CreateAsync(request, currentUserId);
            return Ok(ApiResponse<AppointmentDto>.Success(dto, "预约已创建"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/appointments/{appointmentId}/confirm
    [HttpPut("{appointmentId:int}/confirm")]
    public async Task<IActionResult> Confirm(int appointmentId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _appointmentService.ConfirmAsync(appointmentId, currentUserId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "预约不存在或确认失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "确认成功"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/appointments/{appointmentId}/complete
    [HttpPut("{appointmentId:int}/complete")]
    public async Task<IActionResult> Complete(int appointmentId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _appointmentService.CompleteAsync(appointmentId, currentUserId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "预约不存在或完成失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "已完成"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/appointments/{appointmentId}/cancel
    [HttpPut("{appointmentId:int}/cancel")]
    public async Task<IActionResult> Cancel(int appointmentId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _appointmentService.CancelAsync(appointmentId, currentUserId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "预约不存在或取消失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "取消成功"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // POST /api/appointments/verify
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyConfirmCodeRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _appointmentService.VerifyConfirmCodeAsync(request, currentUserId);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(400, "确认失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "验证成功"));
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
            UnauthorizedAccessException uae => Unauthorized(ApiResponse<object>.Fail(401, uae.Message)),
            ArgumentException ae => BadRequest(ApiResponse<object>.Fail(400, ae.Message)),
            InvalidOperationException ioe => BadRequest(ApiResponse<object>.Fail(400, ioe.Message)),
            _ => StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"))
        };
    }
}
