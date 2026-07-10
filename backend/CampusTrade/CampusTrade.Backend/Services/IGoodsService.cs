using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IGoodsService
{
    Task<GoodsListResult> GetPagedAsync(
        int page, int size,
        int? sellerId,
        string? status,
        int? categoryId,
        string? categoryIds,
        string? keyword,
        decimal? minPrice, decimal? maxPrice,
        string? sortBy, bool ascending);

    Task<GoodsDto?> GetByIdAsync(int goodsId);
    Task<int> CreateAsync(CreateGoodsRequest request, int sellerId);
    Task<bool> UpdateAsync(int goodsId, UpdateGoodsRequest request, int currentUserId, bool isAdmin);
    Task<bool> DeleteAsync(int goodsId, int currentUserId, bool isAdmin);
    Task<bool> AuditAsync(int goodsId, AuditGoodsRequest request, int adminId);
    Task<bool> OfflineAsync(int goodsId, int currentUserId, bool isAdmin);
    Task<bool> IncrementViewCountAsync(int goodsId);

    Task<IEnumerable<GoodsImageDto>> GetImagesAsync(int goodsId);
    Task<int> AddImageAsync(int goodsId, string imageUrl, int sortOrder);
    Task<bool> DeleteImageAsync(int imageId);
}
