using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class GoodsService : IGoodsService
{
    private readonly IGoodsRepository _goodsRepository;
    private readonly IAdminRepository _adminRepository;

    public GoodsService(IGoodsRepository goodsRepository, IAdminRepository adminRepository)
    {
        _goodsRepository = goodsRepository;
        _adminRepository = adminRepository;
    }

    public async Task<GoodsListResult> GetPagedAsync(
        int page, int size,
        int? sellerId,
        string? status,
        int? categoryId, string? keyword,
        decimal? minPrice, decimal? maxPrice,
        string? sortBy, bool ascending)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);
        var (items, total) = await _goodsRepository.GetPagedAsync(
            page, size, sellerId, status, categoryId, keyword, minPrice, maxPrice, sortBy, ascending);
        return new GoodsListResult
        {
            List = items,
            Total = total,
            Page = page,
            Size = size
        };
    }

    public async Task<GoodsDto?> GetByIdAsync(int goodsId)
    {
        return await _goodsRepository.GetByIdAsync(goodsId);
    }

    public async Task<int> CreateAsync(CreateGoodsRequest request, int sellerId)
    {
        if (sellerId <= 0)
        {
            throw new UnauthorizedAccessException("login required");
        }

        ValidateCreateRequest(request);
        return await _goodsRepository.CreateAsync(request, sellerId);
    }

    public async Task<bool> UpdateAsync(int goodsId, UpdateGoodsRequest request, int currentUserId, bool isAdmin)
    {
        var existing = await _goodsRepository.GetByIdAsync(goodsId);
        if (existing == null)
        {
            return false;
        }

        if (!isAdmin && existing.SellerId != currentUserId)
        {
            throw new UnauthorizedAccessException("only the seller or an admin can edit this goods item");
        }

        if (existing.Status == "approved" && !isAdmin)
        {
            throw new InvalidOperationException("approved goods cannot be edited by seller");
        }

        return await _goodsRepository.UpdateAsync(goodsId, request);
    }

    public async Task<bool> DeleteAsync(int goodsId, int currentUserId, bool isAdmin)
    {
        var existing = await _goodsRepository.GetByIdAsync(goodsId);
        if (existing == null)
        {
            return false;
        }

        if (!isAdmin && existing.SellerId != currentUserId)
        {
            throw new UnauthorizedAccessException("only the seller or an admin can delete this goods item");
        }

        return await _goodsRepository.DeleteAsync(goodsId);
    }

    public async Task<bool> AuditAsync(int goodsId, AuditGoodsRequest request, int adminId)
    {
        if (request.Status != "approved" && request.Status != "rejected")
        {
            throw new ArgumentException("audit status must be approved or rejected");
        }

        var existing = await _goodsRepository.GetByIdAsync(goodsId);
        if (existing == null)
        {
            return false;
        }

        return await _goodsRepository.AuditAsync(goodsId, adminId, request.Status, request.Remark);
    }

    public async Task<bool> OfflineAsync(int goodsId, int currentUserId, bool isAdmin)
    {
        var existing = await _goodsRepository.GetByIdAsync(goodsId);
        if (existing == null)
        {
            return false;
        }

        if (!isAdmin && existing.SellerId != currentUserId)
        {
            throw new UnauthorizedAccessException("only the seller or an admin can offline this goods item");
        }

        var success = await _goodsRepository.UpdateStatusAsync(goodsId, "offline");
        if (success && isAdmin)
        {
            await _adminRepository.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                AuditType = "goods_offline",
                TargetId = goodsId,
                Action = "offline",
                Result = "success"
            }, currentUserId);
        }

        return success;
    }

    public async Task<bool> IncrementViewCountAsync(int goodsId)
    {
        return await _goodsRepository.IncrementViewCountAsync(goodsId);
    }

    public async Task<IEnumerable<GoodsImageDto>> GetImagesAsync(int goodsId)
    {
        return await _goodsRepository.GetImagesAsync(goodsId);
    }

    public async Task<int> AddImageAsync(int goodsId, string imageUrl, int sortOrder)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods == null)
        {
            throw new ArgumentException("goods not found");
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new ArgumentException("imageUrl is required");
        }

        return await _goodsRepository.AddImageAsync(goodsId, imageUrl.Trim(), sortOrder);
    }

    public async Task<bool> DeleteImageAsync(int imageId)
    {
        return await _goodsRepository.DeleteImageAsync(imageId);
    }

    private static void ValidateCreateRequest(CreateGoodsRequest request)
    {
        if (request.CategoryId <= 0)
        {
            throw new ArgumentException("categoryId is required");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("title is required");
        }

        if (request.Price < 0)
        {
            throw new ArgumentException("price cannot be negative");
        }

        if (string.IsNullOrWhiteSpace(request.Condition))
        {
            throw new ArgumentException("condition is required");
        }
    }
}