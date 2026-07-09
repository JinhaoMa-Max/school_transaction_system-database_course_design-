using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/goods")]
public class GoodsController : ControllerBase
{
    private readonly IGoodsService _goodsService;
    private readonly IAuthService _authService;
    private readonly IAdminService _adminService;

    public GoodsController(IGoodsService goodsService, IAuthService authService, IAdminService adminService)
    {
        _goodsService = goodsService;
        _authService = authService;
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false)
    {
        try
        {
            var result = await _goodsService.GetPagedAsync(page, size, categoryId, keyword, minPrice, maxPrice, sortBy, ascending);
            return Ok(ApiResponse<GoodsListResult>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpGet("{goodsId:int}")]
    public async Task<IActionResult> GetById(int goodsId)
    {
        try
        {
            var dto = await _goodsService.GetByIdAsync(goodsId);
            if (dto == null)
            {
                return NotFound(ApiResponse<object>.Fail(404, "goods not found"));
            }

            return Ok(ApiResponse<GoodsDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoodsRequest request)
    {
        try
        {
            var currentUserId = RequireCurrentUserId();
            var goodsId = await _goodsService.CreateAsync(request, currentUserId);
            return Ok(ApiResponse<object>.Success(new { id = goodsId }, "goods created"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("{goodsId:int}")]
    public async Task<IActionResult> Update(int goodsId, [FromBody] UpdateGoodsRequest request)
    {
        try
        {
            var currentUserId = RequireCurrentUserId();
            var isAdmin = await _adminService.IsAdminAsync(currentUserId);
            var success = await _goodsService.UpdateAsync(goodsId, request, currentUserId, isAdmin);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(404, "goods not found"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "goods updated"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpDelete("{goodsId:int}")]
    public async Task<IActionResult> Delete(int goodsId)
    {
        try
        {
            var currentUserId = RequireCurrentUserId();
            var isAdmin = await _adminService.IsAdminAsync(currentUserId);
            var success = await _goodsService.DeleteAsync(goodsId, currentUserId, isAdmin);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(404, "goods not found"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "goods deleted"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpGet("{goodsId:int}/images")]
    public async Task<IActionResult> GetImages(int goodsId)
    {
        try
        {
            var images = await _goodsService.GetImagesAsync(goodsId);
            return Ok(ApiResponse<IEnumerable<GoodsImageDto>>.Success(images));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPost("{goodsId:int}/images")]
    public async Task<IActionResult> AddImage(int goodsId, [FromBody] UploadImageRequest request)
    {
        try
        {
            var imageId = await _goodsService.AddImageAsync(goodsId, request.ImageUrl, request.SortOrder);
            return Ok(ApiResponse<object>.Success(new { id = imageId }, "image uploaded"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpDelete("images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        try
        {
            var success = await _goodsService.DeleteImageAsync(imageId);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(404, "image not found"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "image deleted"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("{goodsId:int}/audit")]
    public async Task<IActionResult> Audit(int goodsId, [FromBody] AuditGoodsRequest request)
    {
        try
        {
            var adminId = await _adminService.RequireAdminAsync(ResolveCurrentUserId());
            var success = await _goodsService.AuditAsync(goodsId, request, adminId);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(404, "goods not found"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "goods audited"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("{goodsId:int}/offline")]
    public async Task<IActionResult> Offline(int goodsId)
    {
        try
        {
            var currentUserId = RequireCurrentUserId();
            var isAdmin = await _adminService.IsAdminAsync(currentUserId);
            var success = await _goodsService.OfflineAsync(goodsId, currentUserId, isAdmin);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(404, "goods not found"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "goods offlined"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    [HttpPut("{goodsId:int}/view")]
    public async Task<IActionResult> IncrementView(int goodsId)
    {
        try
        {
            var success = await _goodsService.IncrementViewCountAsync(goodsId);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail(404, "goods not found"));
            }

            return Ok(ApiResponse<object>.Success(new { success = true }, "view count updated"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    private int RequireCurrentUserId()
    {
        return ResolveCurrentUserId() ?? throw new UnauthorizedAccessException("login required");
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
            InvalidOperationException ioe => BadRequest(ApiResponse<object>.Fail(400, ioe.Message)),
            _ => StatusCode(500, ApiResponse<object>.Fail(500, "internal server error"))
        };
    }
}