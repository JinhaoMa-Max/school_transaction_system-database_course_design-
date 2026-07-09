using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IGoodsRepository _goodsRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository, IGoodsRepository goodsRepository)
    {
        _favoriteRepository = favoriteRepository;
        _goodsRepository = goodsRepository;
    }

    public async Task<FavoriteListResult> GetPagedAsync(int? currentUserId, int page, int size)
    {
        var userId = RequireUser(currentUserId);
        if (page < 1) throw new ArgumentException("page must start from 1");
        if (size < 1) throw new ArgumentException("size must be greater than 0");

        var (items, total) = await _favoriteRepository.GetPagedAsync(userId, page, size);
        return new FavoriteListResult
        {
            List = items,
            Total = total,
            Page = page,
            Size = size
        };
    }

    public async Task<FavoriteDto?> GetByIdAsync(int favoriteId, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        if (favoriteId <= 0) throw new ArgumentException("favoriteId is invalid");
        return await _favoriteRepository.GetByIdAsync(userId, favoriteId);
    }

    public async Task<FavoriteDto> AddAsync(CreateFavoriteRequest request, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        if (request.GoodsId <= 0) throw new ArgumentException("goodsId is invalid");

        var goods = await _goodsRepository.GetByIdAsync(request.GoodsId);
        if (goods == null) throw new ArgumentException("goods does not exist");
        if (goods.SellerId == userId) throw new InvalidOperationException("cannot favorite your own goods");

        var existing = await _favoriteRepository.GetByUserAndGoodsAsync(userId, request.GoodsId);
        if (existing != null) return existing;

        var favoriteId = await _favoriteRepository.AddAsync(userId, request.GoodsId);
        return await _favoriteRepository.GetByIdAsync(userId, favoriteId)
            ?? throw new InvalidOperationException("failed to load created favorite");
    }

    public async Task<bool> RemoveAsync(int favoriteId, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        if (favoriteId <= 0) throw new ArgumentException("favoriteId is invalid");
        return await _favoriteRepository.RemoveAsync(userId, favoriteId);
    }

    public async Task<bool> RemoveByGoodsAsync(int goodsId, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        if (goodsId <= 0) throw new ArgumentException("goodsId is invalid");
        return await _favoriteRepository.RemoveByGoodsAsync(userId, goodsId);
    }

    public async Task<FavoriteCheckResult> CheckAsync(int goodsId, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        if (goodsId <= 0) throw new ArgumentException("goodsId is invalid");

        var favorite = await _favoriteRepository.GetByUserAndGoodsAsync(userId, goodsId);
        return new FavoriteCheckResult
        {
            IsFavorite = favorite != null,
            FavoriteId = favorite?.FavoriteId
        };
    }

    private static int RequireUser(int? currentUserId)
    {
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("not logged in");
        }

        return currentUserId.Value;
    }
}
