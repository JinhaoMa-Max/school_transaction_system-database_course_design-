using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class GoodsService : IGoodsService
{
    private readonly IGoodsRepository _goodsRepository;

    public GoodsService(IGoodsRepository goodsRepository)
    {
        _goodsRepository = goodsRepository;
    }

    public async Task<GoodsListResult> GetPagedAsync(
        int page, int size,
        int? categoryId, string? keyword,
        string? sortBy, bool ascending)
    {
        // 可添加额外业务逻辑（如默认排序、过滤敏感词等）
        var (items, total) = await _goodsRepository.GetPagedAsync(
            page, size, categoryId, keyword, sortBy, ascending);
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
        // 业务校验：卖家是否已认证？可在此调用认证服务
        // 校验商品状态默认 pending，由 Repository 处理
        return await _goodsRepository.CreateAsync(request, sellerId);
    }

    public async Task<bool> UpdateAsync(int goodsId, UpdateGoodsRequest request, int currentUserId, bool isAdmin)
    {
        // 仅卖家本人或管理员可编辑
        var existing = await _goodsRepository.GetByIdAsync(goodsId);
        if (existing == null) return false;
        if (!isAdmin && existing.SellerId != currentUserId)
            throw new UnauthorizedAccessException("只有卖家本人或管理员可以编辑商品");

        // 如果商品已审核通过且非管理员，不允许修改（可根据需求调整）
        if (existing.Status == "approved" && !isAdmin)
            throw new InvalidOperationException("商品已审核通过，无法修改，如需修改请联系管理员");

        return await _goodsRepository.UpdateAsync(goodsId, request);
    }

    public async Task<bool> DeleteAsync(int goodsId, int currentUserId, bool isAdmin)
    {
        var existing = await _goodsRepository.GetByIdAsync(goodsId);
        if (existing == null) return false;
        if (!isAdmin && existing.SellerId != currentUserId)
            throw new UnauthorizedAccessException("只有卖家本人或管理员可以删除商品");

        // 软删除：可以改为状态 offline，这里直接物理删除（也可根据需求调整）
        return await _goodsRepository.DeleteAsync(goodsId);
    }

    public async Task<bool> AuditAsync(int goodsId, AuditGoodsRequest request, int adminId)
    {
        // 仅管理员可操作
        if (request.Status != "approved" && request.Status != "rejected")
            throw new ArgumentException("审核状态必须为 'approved' 或 'rejected'");

        // 可记录审核日志（调用 audit_log 写入）
        var success = await _goodsRepository.UpdateStatusAsync(goodsId, request.Status);
        if (success && !string.IsNullOrEmpty(request.Remark))
        {
            // 可写入审核日志表（这里省略，实际应调用 IAuditLogService）
        }
        return success;
    }

    public async Task<bool> OfflineAsync(int goodsId, int currentUserId, bool isAdmin)
    {
        var existing = await _goodsRepository.GetByIdAsync(goodsId);
        if (existing == null) return false;
        if (!isAdmin && existing.SellerId != currentUserId)
            throw new UnauthorizedAccessException("只有卖家本人或管理员可以下架商品");

        return await _goodsRepository.UpdateStatusAsync(goodsId, "offline");
    }

    public async Task<bool> IncrementViewCountAsync(int goodsId)
    {
        return await _goodsRepository.IncrementViewCountAsync(goodsId);
    }

    // ---- 图片操作 ----
    public async Task<IEnumerable<GoodsImageDto>> GetImagesAsync(int goodsId)
    {
        return await _goodsRepository.GetImagesAsync(goodsId);
    }

    public async Task<int> AddImageAsync(int goodsId, string imageUrl, int sortOrder)
    {
        // 可检查商品是否存在
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods == null) throw new ArgumentException("商品不存在");
        return await _goodsRepository.AddImageAsync(goodsId, imageUrl, sortOrder);
    }

    public async Task<bool> DeleteImageAsync(int imageId)
    {
        return await _goodsRepository.DeleteImageAsync(imageId);
    }
}