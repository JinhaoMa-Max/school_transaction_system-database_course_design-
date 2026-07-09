using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IAuthService _authService;

    public OrdersController(IOrderService orderService, IAuthService authService)
    {
        _orderService = orderService;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? role = null)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var result = await _orderService.GetPagedAsync(page, size, status, currentUserId, role);
            return Ok(ApiResponse<OrderListResult>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetById(int orderId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _orderService.GetByIdAsync(orderId, currentUserId);
            if (dto == null) return NotFound(ApiResponse<object>.Fail(404, "order not found"));
            return Ok(ApiResponse<OrderDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _orderService.CreateAsync(request, currentUserId);
            return Ok(ApiResponse<OrderDto>.Success(dto, "order created"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("{orderId:int}")]
    public async Task<IActionResult> Update(int orderId, [FromBody] UpdateOrderRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _orderService.UpdateAsync(orderId, request, currentUserId);
            return Ok(ApiResponse<OrderDto>.Success(dto, "order updated"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("{orderId:int}/cancel")]
    public async Task<IActionResult> Cancel(int orderId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _orderService.CancelAsync(orderId, currentUserId);
            if (!success) return NotFound(ApiResponse<object>.Fail(404, "order not found or cancel failed"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "order cancelled"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("{orderId:int}/start-meet")]
    public async Task<IActionResult> StartMeet(int orderId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _orderService.StartMeetAsync(orderId, currentUserId);
            if (!success) return NotFound(ApiResponse<object>.Fail(404, "order not found or start meet failed"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "meet started"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("{orderId:int}/complete")]
    public async Task<IActionResult> Complete(
        int orderId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CompleteOrderRequest? request = null)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _orderService.CompleteAsync(orderId, request?.ConfirmCode, currentUserId);
            if (!success) return NotFound(ApiResponse<object>.Fail(404, "order not found or complete failed"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "order completed"));
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
        if (string.IsNullOrWhiteSpace(value)) return null;
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
            _ => StatusCode(500, ApiResponse<object>.Fail(500, "internal server error"))
        };
    }
}
