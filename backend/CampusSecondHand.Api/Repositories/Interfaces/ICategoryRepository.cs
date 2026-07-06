using CampusSecondHand.Api.Models.Entities;

namespace CampusSecondHand.Api.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<int> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> HasChildrenAsync(int id);
    Task<bool> HasGoodsAsync(int id);
}
