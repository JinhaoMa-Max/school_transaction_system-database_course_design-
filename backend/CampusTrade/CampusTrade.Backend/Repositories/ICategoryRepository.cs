using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

public interface ICategoryRepository
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int categoryId);
    Task<int> CreateAsync(CreateCategoryRequest request);
    Task<bool> UpdateAsync(int categoryId, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int categoryId);
    Task<bool> HasChildrenAsync(int categoryId);
    Task<int> GetMaxSortOrderAsync(int? parentId);
    Task<bool> HasGoodsAsync(int categoryId);
}