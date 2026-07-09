using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

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

    // GET /api/orders
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? status = null)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var result = await _orderService.GetPagedAsync(page, size, status, currentUserId);
            return Ok(ApiResponse<OrderListResult>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // GET /api/orders/{orderId}
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetById(int orderId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _orderService.GetByIdAsync(orderId, currentUserId);
            if (dto == null)
                return NotFound(ApiResponse<object>.Fail(404, "订单不存在"));
            return Ok(ApiResponse<TradeOrderDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // POST /api/orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _orderService.CreateAsync(request, currentUserId);
            return Ok(ApiResponse<TradeOrderDto>.Success(dto, "下单成功"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/orders/{orderId}
    [HttpPut("{orderId:int}")]
    public async Task<IActionResult> Update(int orderId, [FromBody] UpdateOrderRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _orderService.UpdateAsync(orderId, request, currentUserId);
            return Ok(ApiResponse<TradeOrderDto>.Success(dto, "更新成功"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/orders/{orderId}/cancel
    [HttpPut("{orderId:int}/cancel")]
    public async Task<IActionResult> Cancel(int orderId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _orderService.CancelAsync(orderId, currentUserId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "订单不存在或取消失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "取消成功"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/orders/{orderId}/complete
    [HttpPut("{orderId:int}/complete")]
    public async Task<IActionResult> Complete(int orderId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _orderService.CompleteAsync(orderId, currentUserId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "订单不存在或完成失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "交易已完成"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/orders/{orderId}/start-meet
    [HttpPut("{orderId:int}/start-meet")]
    public async Task<IActionResult> StartMeet(int orderId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _orderService.StartMeetAsync(orderId, currentUserId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "订单不存在或操作失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "已开始面交"));
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

