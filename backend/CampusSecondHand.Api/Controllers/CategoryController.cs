using CampusSecondHand.Api.Common;
using CampusSecondHand.Api.Models.Dtos;
using CampusSecondHand.Api.Models.Requests;
using CampusSecondHand.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CampusSecondHand.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<List<CategoryDto>>.Ok(categories));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category is null)
        {
            return NotFound(ApiResponse<CategoryDto>.Fail("Category not found."));
        }

        return Ok(ApiResponse<CategoryDto>.Ok(category));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateAsync(request);
        var response = ApiResponse<CategoryDto>.Ok(category, "Created");

        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, UpdateCategoryRequest request)
    {
        var category = await _categoryService.UpdateAsync(id, request);
        return Ok(ApiResponse<CategoryDto>.Ok(category, "Updated"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }, "Deleted"));
    }
}
