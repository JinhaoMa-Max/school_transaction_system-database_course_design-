using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int categoryId);
    Task<int> CreateAsync(CreateCategoryRequest request);
    Task<bool> UpdateAsync(int categoryId, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int categoryId);
}