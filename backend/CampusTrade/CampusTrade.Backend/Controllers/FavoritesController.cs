using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly IAuthService _authService;

    public FavoritesController(IFavoriteService favoriteService, IAuthService authService)
    {
        _favoriteService = favoriteService;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        try
        {
            var result = await _favoriteService.GetPagedAsync(ResolveCurrentUserId(), page, size);
            return Ok(ApiResponse<FavoriteListResult>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpGet("check")]
    public async Task<IActionResult> Check([FromQuery] int goodsId)
    {
        try
        {
            var result = await _favoriteService.CheckAsync(goodsId, ResolveCurrentUserId());
            return Ok(ApiResponse<FavoriteCheckResult>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpGet("{favoriteId:int}")]
    public async Task<IActionResult> GetById(int favoriteId)
    {
        try
        {
            var favorite = await _favoriteService.GetByIdAsync(favoriteId, ResolveCurrentUserId());
            if (favorite == null)
            {
                return NotFound(ApiResponse<object>.Fail(404, "favorite does not exist"));
            }

            return Ok(ApiResponse<FavoriteDto>.Success(favorite));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFavoriteRequest request)
    {
        try
        {
            var favorite = await _favoriteService.AddAsync(request, ResolveCurrentUserId());
            return Ok(ApiResponse<FavoriteDto>.Success(favorite, "favorite created"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpDelete("{favoriteId:int}")]
    public async Task<IActionResult> Delete(int favoriteId)
    {
        try
        {
            var success = await _favoriteService.RemoveAsync(favoriteId, ResolveCurrentUserId());
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(404, "favorite does not exist"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "favorite deleted"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpDelete("goods/{goodsId:int}")]
    public async Task<IActionResult> DeleteByGoods(int goodsId)
    {
        try
        {
            var success = await _favoriteService.RemoveByGoodsAsync(goodsId, ResolveCurrentUserId());
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(404, "favorite does not exist"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "favorite deleted"));
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
            UnauthorizedAccessException uae => Unauthorized(ApiResponse<object>.Fail(401, uae.Message)),
            ArgumentException ae => BadRequest(ApiResponse<object>.Fail(400, ae.Message)),
            InvalidOperationException ioe => BadRequest(ApiResponse<object>.Fail(400, ioe.Message)),
            _ => StatusCode(500, ApiResponse<object>.Fail(500, "server error"))
        };
    }
}
