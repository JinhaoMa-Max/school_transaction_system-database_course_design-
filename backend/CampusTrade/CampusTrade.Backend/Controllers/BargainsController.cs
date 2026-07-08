using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/bargains")]
public class BargainsController : ControllerBase
{
    private readonly IBargainService _bargainService;
    private readonly IAuthService _authService;

    public BargainsController(IBargainService bargainService, IAuthService authService)
    {
        _bargainService = bargainService;
        _authService = authService;
    }

    // GET /api/bargains
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] int? goodsId = null,
        [FromQuery] int? buyerId = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var result = await _bargainService.GetPagedAsync(page, size, goodsId, buyerId, status);
            return Ok(ApiResponse<BargainListResult>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // GET /api/bargains/{bargainId}
    [HttpGet("{bargainId:int}")]
    public async Task<IActionResult> GetById(int bargainId)
    {
        try
        {
            var dto = await _bargainService.GetByIdAsync(bargainId);
            if (dto == null)
                return NotFound(ApiResponse<object>.Fail(404, "议价不存在"));
            return Ok(ApiResponse<BargainOfferDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // POST /api/bargains
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBargainRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _bargainService.CreateAsync(request, currentUserId);
            return Ok(ApiResponse<BargainOfferDto>.Success(dto, "议价已创建"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/bargains/{bargainId}/handle
    [HttpPut("{bargainId:int}/handle")]
    public async Task<IActionResult> Handle(int bargainId, [FromBody] HandleBargainRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var dto = await _bargainService.HandleAsync(bargainId, request, currentUserId);
            return Ok(ApiResponse<BargainOfferDto>.Success(dto, "处理成功"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // PUT /api/bargains/{bargainId}/close
    [HttpPut("{bargainId:int}/close")]
    public async Task<IActionResult> Close(int bargainId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _bargainService.CloseAsync(bargainId, currentUserId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "议价不存在或关闭失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "关闭成功"));
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
