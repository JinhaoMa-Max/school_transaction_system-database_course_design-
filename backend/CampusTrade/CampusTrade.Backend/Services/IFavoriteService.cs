using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IFavoriteService
{
    Task<FavoriteListResult> GetPagedAsync(int? currentUserId, int page, int size);

    Task<FavoriteDto?> GetByIdAsync(int favoriteId, int? currentUserId);

    Task<FavoriteDto> AddAsync(CreateFavoriteRequest request, int? currentUserId);

    Task<bool> RemoveAsync(int favoriteId, int? currentUserId);

    Task<bool> RemoveByGoodsAsync(int goodsId, int? currentUserId);

    Task<FavoriteCheckResult> CheckAsync(int goodsId, int? currentUserId);
}
