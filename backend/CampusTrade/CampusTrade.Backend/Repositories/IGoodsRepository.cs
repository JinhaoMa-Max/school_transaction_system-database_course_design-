using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

public interface IGoodsRepository
{
    Task<(List<GoodsDto> Items, int Total)> GetPagedAsync(
        int page, int size,
        int? categoryId, string? keyword,
        string? sortBy, bool ascending);

    Task<GoodsDto?> GetByIdAsync(int goodsId);
    Task<int> CreateAsync(CreateGoodsRequest request, int sellerId);
    Task<bool> UpdateAsync(int goodsId, UpdateGoodsRequest request);
    Task<bool> DeleteAsync(int goodsId);
    Task<bool> UpdateStatusAsync(int goodsId, string newStatus);
    Task<bool> IncrementViewCountAsync(int goodsId);

    Task<IEnumerable<GoodsImageDto>> GetImagesAsync(int goodsId);
    Task<int> AddImageAsync(int goodsId, string imageUrl, int sortOrder);
    Task<bool> DeleteImageAsync(int imageId);
}