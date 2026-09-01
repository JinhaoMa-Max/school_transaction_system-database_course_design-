using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IAuthService _authService;
    private readonly IAdminService _adminService;

    public CategoriesController(
        ICategoryService categoryService,
        IAuthService authService,
        IAdminService adminService)
    {
        _categoryService = categoryService;
        _authService = authService;
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<object>.Success(categories));
    }

    [HttpGet("{categoryId:int}")]
    public async Task<IActionResult> GetById(int categoryId)
    {
        var dto = await _categoryService.GetByIdAsync(categoryId);
        if (dto == null)
            return NotFound(ApiResponse<object>.Fail(404, "分类不存在"));
        return Ok(ApiResponse<CategoryDto>.Success(dto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        try
        {
            await _adminService.RequireAdminAsync(ResolveCurrentUserId());
            var id = await _categoryService.CreateAsync(request);
            return Ok(ApiResponse<object>.Success(new { id = id }, "分类创建成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(403, ex.Message));
        }
    }

    [HttpPut("{categoryId:int}")]
    public async Task<IActionResult> Update(int categoryId, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            await _adminService.RequireAdminAsync(ResolveCurrentUserId());
            var success = await _categoryService.UpdateAsync(categoryId, request);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "分类不存在或更新失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "分类更新成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(403, ex.Message));
        }
    }

    [HttpDelete("{categoryId:int}")]
    public async Task<IActionResult> Delete(int categoryId)
    {
        try
        {
            await _adminService.RequireAdminAsync(ResolveCurrentUserId());
            var success = await _categoryService.DeleteAsync(categoryId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "分类不存在或删除失败"));
            return Ok(ApiResponse<object>.Success(new { success = true }, "分类删除成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<object>.Fail(404, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(403, ex.Message));
        }
    }

    private int? ResolveCurrentUserId()
    {
        var value = Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        var token = value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : value.Trim();
        return _authService.TryGetUserIdFromToken(token);
    }
}
