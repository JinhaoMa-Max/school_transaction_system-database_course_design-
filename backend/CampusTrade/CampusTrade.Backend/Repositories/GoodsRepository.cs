using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;

namespace CampusTrade.Backend.Repositories;

public class GoodsRepository : IGoodsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GoodsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ==================== 商品核心 CRUD ====================

    public async Task<(List<GoodsDto> Items, int Total)> GetPagedAsync(
        int page, int size, int? categoryId, string? keyword, string? sortBy, bool ascending)
    {
        // TODO: 实现分页查询
        throw new NotImplementedException("GetPagedAsync 待实现");
    }

    public async Task<GoodsDto?> GetByIdAsync(int goodsId)
    {
        // TODO: 实现查询单个商品
        throw new NotImplementedException("GetByIdAsync 待实现");
    }

    public async Task<int> CreateAsync(CreateGoodsRequest request, int sellerId)
    {
        // TODO: 实现插入商品，返回自增ID
        throw new NotImplementedException("CreateAsync 待实现");
    }

    public async Task<bool> UpdateAsync(int goodsId, UpdateGoodsRequest request)
    {
        // TODO: 实现更新商品
        throw new NotImplementedException("UpdateAsync 待实现");
    }

    public async Task<bool> DeleteAsync(int goodsId)
    {
        // TODO: 实现删除商品
        throw new NotImplementedException("DeleteAsync 待实现");
    }

    public async Task<bool> UpdateStatusAsync(int goodsId, string newStatus)
    {
        // TODO: 实现更新状态
        throw new NotImplementedException("UpdateStatusAsync 待实现");
    }

    public async Task<bool> IncrementViewCountAsync(int goodsId)
    {
        // TODO: 实现浏览量+1
        throw new NotImplementedException("IncrementViewCountAsync 待实现");
    }

    // ==================== 商品图片管理 ====================

    public async Task<IEnumerable<GoodsImageDto>> GetImagesAsync(int goodsId)
    {
        // TODO: 实现查询图片列表
        throw new NotImplementedException("GetImagesAsync 待实现");
    }

    public async Task<int> AddImageAsync(int goodsId, string imageUrl, int sortOrder)
    {
        // TODO: 实现插入图片，返回自增ID
        throw new NotImplementedException("AddImageAsync 待实现");
    }

    public async Task<bool> DeleteImageAsync(int imageId)
    {
        // TODO: 实现删除图片
        throw new NotImplementedException("DeleteImageAsync 待实现");
    }

    // ==================== 辅助业务方法（新增） ====================

    public async Task<bool> IsOwnedByUserAsync(int goodsId, int sellerId)
    {
        // TODO: 实现校验卖家归属
        // 临时返回 false 以便编译
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> IsAvailableForPurchaseAsync(int goodsId)
    {
        // TODO: 实现检查商品是否可购买（状态为 'approved'）
        await Task.CompletedTask;
        return false;
    }

    public async Task<string?> GetStatusAsync(int goodsId)
    {
        // TODO: 实现查询商品状态
        await Task.CompletedTask;
        return null;
    }

    public async Task<bool> LockForPurchaseAsync(int goodsId)
    {
        // TODO: 实现原子锁定（状态 'approved' -> 'locked'）
        // 注意：条件更新，影响行数为1表示成功
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> UnlockGoodsAsync(int goodsId)
    {
        // TODO: 实现释放锁定（状态 'locked' -> 'approved'）
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> MarkAsSoldAsync(int goodsId)
    {
        // TODO:实现标记售出（状态 -> 'sold'）
        await Task.CompletedTask;
        return false;
    }
}