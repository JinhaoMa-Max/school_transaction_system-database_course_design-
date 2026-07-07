using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CampusTrade.Backend.Models.Common;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Models.Entities;
using CampusTrade.Backend.Models.Requests;
using CampusTrade.Backend.Repositories.Interfaces;
using CampusTrade.Backend.Services.Interfaces;

namespace CampusTrade.Backend.Services;

public class GoodsService : IGoodsService
{
    private readonly IGoodsRepository _goodsRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GoodsService(IGoodsRepository goodsRepository, ICategoryRepository categoryRepository)
    {
        _goodsRepository = goodsRepository;
        _categoryRepository = categoryRepository;
    }

    // ========================== 核心业务 ==========================

    public async Task<GoodsDto> GetByIdAsync(int id)
    {
        var goods = await _goodsRepository.GetByIdAsync(id);
        if (goods is null)
            throw new BusinessException("商品不存在");

        return ToDto(goods);
    }

    public async Task<GoodsListResult> SearchAsync(GoodsQueryRequest request)
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
            request.Size
        );

        var total = await _goodsRepository.CountSearchAsync(
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
            // 新 DTO 只有单张图片 URL，取第一张
            var images = await _goodsRepository.GetImagesByGoodsIdAsync(goods.GoodsId);
            if (images.Count > 0)
                dto.ImageUrl = images[0].ImageUrl;
            dtos.Add(dto);
        }

        return new GoodsListResult
        {
            List = dtos,
            Total = total,
            Page = request.Page,
            Size = request.Size
        };
    }

    public async Task<int> PublishAsync(PublishGoodsRequest request, int sellerId)
    {
        if (!await _categoryRepository.ExistsAsync(request.CategoryId))
            throw new BusinessException("分类不存在");

        if (request.Price <= 0)
            throw new BusinessException("价格必须大于0");

        var goods = new Goods
        {
            SellerId = sellerId,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Condition = request.Condition,
            GoodsStatus = "pending",
            ViewCount = 0
        };

        var goodsId = await _goodsRepository.CreateAsync(goods);

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

        if (goods.SellerId != userId)
            throw new BusinessException("无权编辑此商品");

        if (goods.GoodsStatus != "offline" && goods.GoodsStatus != "rejected")
            throw new BusinessException("只有已下架或已驳回的商品才能编辑");

        if (!await _categoryRepository.ExistsAsync(request.CategoryId))
            throw new BusinessException("分类不存在");

        goods.CategoryId = request.CategoryId;
        goods.Title = request.Title.Trim();
        goods.Description = request.Description?.Trim();
        goods.Price = request.Price;
        goods.Condition = request.Condition;

        await _goodsRepository.UpdateAsync(goods);
        await _goodsRepository.UpdateStatusAsync(goodsId, "pending");

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
        await _goodsRepository.UpdateStatusAsync(goodsId, newStatus);
    }

    public async Task DeleteAsync(int goodsId, int userId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null)
            throw new BusinessException("商品不存在");

        if (goods.SellerId != userId)
            throw new BusinessException("无权删除此商品");

        if (goods.GoodsStatus != "offline" && goods.GoodsStatus != "rejected")
            throw new BusinessException("只有已下架或已驳回的商品才能删除");

        var deleted = await _goodsRepository.DeleteAsync(goodsId);
        if (!deleted)
            throw new BusinessException("删除失败，请稍后重试");
    }

    // ========================== 图片管理 ==========================

    public async Task<List<GoodsImage>> GetImagesAsync(int goodsId)
    {
        if (!await _goodsRepository.ExistsAsync(goodsId))
            throw new BusinessException("商品不存在");

        return await _goodsRepository.GetImagesByGoodsIdAsync(goodsId);
    }

    public async Task<GoodsImage> AddImageAsync(int goodsId, AddGoodsImageRequest request)
    {
        if (!await _goodsRepository.ExistsAsync(goodsId))
            throw new BusinessException("商品不存在");

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
            throw new BusinessException("图片URL不能为空");

        var image = new GoodsImage
        {
            GoodsId = goodsId,
            ImageUrl = request.ImageUrl.Trim(),
            SortOrder = request.SortOrder
        };

        await _goodsRepository.AddImageAsync(image);
        return image;
    }

    public async Task DeleteImageAsync(int imageId, int userId)
    {
        var image = await _goodsRepository.GetImageByIdAsync(imageId);
        if (image is null)
            throw new BusinessException("图片不存在");

        var goods = await _goodsRepository.GetByIdAsync(image.GoodsId);
        if (goods is null)
            throw new BusinessException("关联商品不存在");

        if (goods.SellerId != userId)
            throw new BusinessException("无权删除此图片");

        var deleted = await _goodsRepository.DeleteImageAsync(imageId);
        if (!deleted)
            throw new BusinessException("删除图片失败");
    }

    // ========================== 其他 ==========================

    public async Task IncrementViewAsync(int goodsId)
    {
        if (!await _goodsRepository.ExistsAsync(goodsId))
            throw new BusinessException("商品不存在");

        await _goodsRepository.IncrementViewCountAsync(goodsId);
    }

    // ========================== 交易模块调用 ==========================

    public async Task<bool> LockGoodsAsync(int goodsId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null || goods.GoodsStatus != "approved")
            return false;

        return await _goodsRepository.UpdateStatusAsync(goodsId, "locked");
    }

    public async Task<bool> UnlockGoodsAsync(int goodsId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null || goods.GoodsStatus != "locked")
            return false;

        return await _goodsRepository.UpdateStatusAsync(goodsId, "approved");
    }

    public async Task<bool> MarkAsSoldAsync(int goodsId)
    {
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods is null || goods.GoodsStatus != "locked")
            return false;

        return await _goodsRepository.UpdateStatusAsync(goodsId, "sold");
    }

    // ========================== 辅助方法 ==========================

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
            Status = goods.GoodsStatus,    // 新 DTO 用的是 Status
            ViewCount = goods.ViewCount,
            PublishTime = goods.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),  // 新 DTO 用的是 PublishTime（string）
            ImageUrl = null,  // 外层会填充
            SellerNickname = null,  // 需要从 User 表联查，暂留空
            CategoryName = null  // 需要从 Category 表联查，暂留空
        };
    }
}