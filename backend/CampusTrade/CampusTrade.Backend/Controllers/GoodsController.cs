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

    public GoodsController(IGoodsService goodsService)
    {
        _goodsService = goodsService;
    }

    // GET /api/goods
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false)
    {
        var result = await _goodsService.GetPagedAsync(page, size, categoryId, keyword, sortBy, ascending);
        return Ok(ApiResponse<GoodsListResult>.Success(result));
    }

    // GET /api/goods/{id}
    [HttpGet("{goodsId:int}")]
    public async Task<IActionResult> GetById(int goodsId)
    {
        var dto = await _goodsService.GetByIdAsync(goodsId);
        if (dto == null)
            return NotFound(ApiResponse<object>.Fail(404, "商品不存在"));
        return Ok(ApiResponse<GoodsDto>.Success(dto));
    }

    // POST /api/goods
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoodsRequest request)
    {
        // 实际应从 JWT 中获取当前用户ID，这里暂用 1 模拟（需替换）
        int currentUserId = 1; // TODO: 从 HttpContext.User 获取
        var goodsId = await _goodsService.CreateAsync(request, currentUserId);
        return Ok(ApiResponse<object>.Success(new { id = goodsId }, "商品发布成功"));
    }

    // PUT /api/goods/{id}
    [HttpPut("{goodsId:int}")]
    public async Task<IActionResult> Update(int goodsId, [FromBody] UpdateGoodsRequest request)
    {
        int currentUserId = 1; // TODO: 从认证获取
        bool isAdmin = false;  // TODO: 检查角色
        var success = await _goodsService.UpdateAsync(goodsId, request, currentUserId, isAdmin);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "商品不存在或更新失败"));
        return Ok(ApiResponse<object>.Success(new { success = true }, "更新成功"));
    }

    // DELETE /api/goods/{id}
    [HttpDelete("{goodsId:int}")]
    public async Task<IActionResult> Delete(int goodsId)
    {
        int currentUserId = 1;
        bool isAdmin = false;
        var success = await _goodsService.DeleteAsync(goodsId, currentUserId, isAdmin);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "商品不存在或删除失败"));
        return Ok(ApiResponse<object>.Success(new { success = true }, "删除成功"));
    }

    // GET /api/goods/{id}/images
    [HttpGet("{goodsId:int}/images")]
    public async Task<IActionResult> GetImages(int goodsId)
    {
        var images = await _goodsService.GetImagesAsync(goodsId);
        return Ok(ApiResponse<IEnumerable<GoodsImageDto>>.Success(images));
    }

    // POST /api/goods/{id}/images
    [HttpPost("{goodsId:int}/images")]
    public async Task<IActionResult> AddImage(int goodsId, [FromBody] UploadImageRequest request)
    {
        var imageId = await _goodsService.AddImageAsync(goodsId, request.ImageUrl, request.SortOrder);
        return Ok(ApiResponse<object>.Success(new { id = imageId }, "图片上传成功"));
    }

    // DELETE /api/goods/images/{imageId}
    [HttpDelete("images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var success = await _goodsService.DeleteImageAsync(imageId);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "图片不存在"));
        return Ok(ApiResponse<object>.Success(new { success = true }, "图片删除成功"));
    }

    // PUT /api/goods/{id}/audit
    [HttpPut("{goodsId:int}/audit")]
    public async Task<IActionResult> Audit(int goodsId, [FromBody] AuditGoodsRequest request)
    {
        int adminId = 1; // TODO: 从认证获取管理员ID
        var success = await _goodsService.AuditAsync(goodsId, request, adminId);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "商品不存在或审核失败"));
        return Ok(ApiResponse<object>.Success(new { success = true }, "审核完成"));
    }

    // PUT /api/goods/{id}/offline
    [HttpPut("{goodsId:int}/offline")]
    public async Task<IActionResult> Offline(int goodsId)
    {
        int currentUserId = 1;
        bool isAdmin = false;
        var success = await _goodsService.OfflineAsync(goodsId, currentUserId, isAdmin);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "商品不存在或下架失败"));
        return Ok(ApiResponse<object>.Success(new { success = true }, "商品已下架"));
    }

    // PUT /api/goods/{id}/view
    [HttpPut("{goodsId:int}/view")]
    public async Task<IActionResult> IncrementView(int goodsId)
    {
        var success = await _goodsService.IncrementViewCountAsync(goodsId);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "商品不存在"));
        return Ok(ApiResponse<object>.Success(new { success = true }, "浏览量已更新"));
    }
}