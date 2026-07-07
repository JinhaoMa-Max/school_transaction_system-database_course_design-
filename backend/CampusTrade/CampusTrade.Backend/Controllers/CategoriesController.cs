using CampusTrade.Backend.Models;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        return Ok(ApiResponse<object>.Success(categories));
    }
}