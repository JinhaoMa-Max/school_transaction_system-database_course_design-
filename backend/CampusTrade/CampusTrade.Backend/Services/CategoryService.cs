using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }
}