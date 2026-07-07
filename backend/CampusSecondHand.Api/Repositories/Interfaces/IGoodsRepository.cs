using CampusSecondHand.Api.Models.Entities;

namespace CampusSecondHand.Api.Repositories.Interfaces;

public interface IGoodsRepository
{
    Task<List<Goods>> GetAllAsync(int page, int size);
    Task<int> GetCountAsync();
    Task<Goods?> GetByIdAsync(int id);
}
