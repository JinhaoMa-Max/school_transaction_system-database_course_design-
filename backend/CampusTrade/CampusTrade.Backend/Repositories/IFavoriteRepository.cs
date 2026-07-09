using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

public interface IFavoriteRepository
{
    Task<(List<FavoriteDto> Items, int Total)> GetPagedAsync(int userId, int page, int size);

    Task<FavoriteDto?> GetByIdAsync(int userId, int favoriteId);

    Task<FavoriteDto?> GetByUserAndGoodsAsync(int userId, int goodsId);

    Task<int> AddAsync(int userId, int goodsId);

    Task<bool> RemoveAsync(int userId, int favoriteId);

    Task<bool> RemoveByGoodsAsync(int userId, int goodsId);

    Task<bool> IsFavoritedAsync(int userId, int goodsId);

    Task<string> GetFavoriteGoodsIdsAsync(int userId);
}
