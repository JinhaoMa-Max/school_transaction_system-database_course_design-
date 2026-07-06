using CampusSecondHand.Api.Models.Dtos;
using CampusSecondHand.Api.Models.Requests;

namespace CampusSecondHand.Api.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request);
    Task DeleteAsync(int id);
}
