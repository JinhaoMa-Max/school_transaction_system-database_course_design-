using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CampusSecondHand.Api.Common;
using CampusSecondHand.Api.Models.Dtos;
using CampusSecondHand.Api.Models.Entities;
using CampusSecondHand.Api.Models.Requests;
using CampusSecondHand.Api.Repositories.Interfaces;
using CampusSecondHand.Api.Services.Interfaces;

namespace CampusSecondHand.Api.Services;

public class GoodsService : IGoodsService
{
    private readonly IGoodsRepository _goodsRepository;
    private readonly ICategoryRepository _categoryRepository;

    // 这里需要注入 ICategoryRepository 来校验分类是否存在
    public GoodsService(IGoodsRepository goodsRepository, ICategoryRepository categoryRepository)
    {
        _goodsRepository = goodsRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<GoodsDto> GetByIdAsync(int id)
    {
        var goods = await _goodsRepository.GetByIdAsync(id);
        if (goods is null)
            throw new BusinessException("商品不存在");

        return ToDto(goods);
    }

    public async Task<GoodsListResponseDto> SearchAsync(SearchGoodsRequest request)
    {
        var items = await _goodsRepository.SearchAsync(
            request.Keyword,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.Condition,
            request.SortBy,
            request.Ascending,
            request.Page,
            request.PageSize
        );

        var totalCount = await _goodsRepository.CountSearchAsync(
            request.Keyword,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.Condition
        );

        var dtos = new List<GoodsDto>();
        foreach (var goods in items)
        {
            var dto = ToDto(goods);
            // 查图片
            var images = await _goodsRepository.GetImagesByGoodsIdAsync(goods.GoodsId);
            dto.ImageUrls = images.ConvertAll(i => i.ImageUrl);
            dtos.Add(dto);
        }

        return new GoodsListResponseDto
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<int> PublishAsync(PublishGoodsRequest request, int sellerId)
    {
        // 1. 校验分类是否存在
        if (!await _categoryRepository.ExistsAsync(request.CategoryId))
            throw new BusinessException("分类不存在");

        // 2. 校验价格
        if (request.Price <= 0)
            throw new BusinessException("价格必须大于0");

        // 3. 创建商品实体
        var goods = new Goods
        {
            SellerId = sellerId,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Condition = request.Condition,
            GoodsStatus = "pending", // 待审核
            ViewCount = 0
        };

        // 4. 入库
        var goodsId = await _goodsRepository.CreateAsync(goods);

        // 5. 处理图片
        if (request.ImageUrls != null && request.ImageUrls.Count > 0)
        {
            for (int i = 0; i < request.ImageUrls.Count; i++)
            {
                await _goodsRepository.AddImageAsync(new GoodsImage
                {
                    GoodsId = goodsId,
                    ImageUrl = request.ImageUrls[i],
                    SortOrder = i
                });
            }
        }

        return goodsId;
    }

    public async Task UpdateAsync(int goodsId, UpdateGoodsRequest request, int userId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null)
            throw new BusinessException("商品不存在");

        // 只能编辑自己的商品
        if (goods.SellerId != userId)
            throw new BusinessException("无权编辑此商品");

        // 只有下架或驳回状态才能编辑并重新提交审核
        if (goods.GoodsStatus != "offline" && goods.GoodsStatus != "rejected")
            throw new BusinessException("只有已下架或已驳回的商品才能编辑");

        // 校验新分类
        if (!await _categoryRepository.ExistsAsync(request.CategoryId))
            throw new BusinessException("分类不存在");

        // 更新信息
        goods.CategoryId = request.CategoryId;
        goods.Title = request.Title.Trim();
        goods.Description = request.Description?.Trim();
        goods.Price = request.Price;
        goods.Condition = request.Condition;

        await _goodsRepository.UpdateAsync(goods);

        // 重置状态为待审核
        await _goodsRepository.UpdateStatusAsync(goodsId, "pending");

        // 更新图片（先删再加）
        if (request.ImageUrls != null)
        {
            await _goodsRepository.DeleteImagesByGoodsIdAsync(goodsId);
            for (int i = 0; i < request.ImageUrls.Count; i++)
            {
                await _goodsRepository.AddImageAsync(new GoodsImage
                {
                    GoodsId = goodsId,
                    ImageUrl = request.ImageUrls[i],
                    SortOrder = i
                });
            }
        }
    }

    public async Task OfflineAsync(int goodsId, int userId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null)
            throw new BusinessException("商品不存在");

        if (goods.SellerId != userId)
            throw new BusinessException("无权下架此商品");

        // 已售出、已锁定不能下架
        if (goods.GoodsStatus == "sold")
            throw new BusinessException("已售出的商品不能下架");
        if (goods.GoodsStatus == "locked")
            throw new BusinessException("交易进行中的商品不能下架");

        await _goodsRepository.UpdateStatusAsync(goodsId, "offline");
    }

    public async Task AuditAsync(int goodsId, bool approved, string? remark, int adminId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null)
            throw new BusinessException("商品不存在");

        if (goods.GoodsStatus != "pending")
            throw new BusinessException("只有待审核的商品才能审核");

        var newStatus = approved ? "approved" : "rejected";

        // 更新状态
        await _goodsRepository.UpdateStatusAsync(goodsId, newStatus);

        // TODO: 写入审核日志 (audit_log)
        // 这里建议你稍后实现 AuditLogRepository 并注入，先在表中记录一下动作
        // await _auditLogRepository.CreateAsync(new AuditLog { ... });
        // 当前先不阻塞流程，留有接口
    }

    // ---- 给交易模块调用的方法 ----
    public async Task<bool> LockGoodsAsync(int goodsId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null) return false;

        // 只有 approved 状态才能被锁定下单
        if (goods.GoodsStatus != "approved")
            return false;

        return await _goodsRepository.UpdateStatusAsync(goodsId, "locked");
    }

    public async Task<bool> UnlockGoodsAsync(int goodsId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null) return false;

        // 只有 locked 状态才能解锁
        if (goods.GoodsStatus != "locked")
            return false;

        return await _goodsRepository.UpdateStatusAsync(goodsId, "approved");
    }

    public async Task<bool> MarkAsSoldAsync(int goodsId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null) return false;

        // 只有 locked 状态才能变为已售出
        if (goods.GoodsStatus != "locked")
            return false;

        return await _goodsRepository.UpdateStatusAsync(goodsId, "sold");
    }

    // ---- 辅助方法 ----
    private static GoodsDto ToDto(Goods goods)
    {
        return new GoodsDto
        {
            GoodsId = goods.GoodsId,
            SellerId = goods.SellerId,
            CategoryId = goods.CategoryId,
            Title = goods.Title,
            Description = goods.Description,
            Price = goods.Price,
            Condition = goods.Condition,
            GoodsStatus = goods.GoodsStatus,
            ViewCount = goods.ViewCount,
            CreatedAt = goods.CreatedAt,
            UpdatedAt = goods.UpdatedAt,
            ImageUrls = new List<string>()
        };
    }
}