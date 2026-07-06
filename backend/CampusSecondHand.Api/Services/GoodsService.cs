using CampusSecondHand.Api.Models.Dtos;
using CampusSecondHand.Api.Repositories.Interfaces;
using CampusSecondHand.Api.Services.Interfaces;

namespace CampusSecondHand.Api.Services;

public class GoodsService : IGoodsService
{
    private readonly IGoodsRepository _goodsRepository;

    public GoodsService(IGoodsRepository goodsRepository)
    {
        _goodsRepository = goodsRepository;
    }

    public async Task<GoodsListResult> GetListAsync(int page, int size)
    {
        var total = await _goodsRepository.GetCountAsync();
        var goods = await _goodsRepository.GetAllAsync(page, size);

        return new GoodsListResult
        {
            List = goods.Select(ToDto).ToList(),
            Total = total,
            Page = page,
            Size = size
        };
    }

    public async Task<GoodsDto?> GetByIdAsync(int id)
    {
        var goods = await _goodsRepository.GetByIdAsync(id);
        return goods is null ? null : ToDto(goods);
    }

    private static GoodsDto ToDto(Models.Entities.Goods g)
    {
        return new GoodsDto
        {
            GoodsId = g.GoodsId,
            SellerId = g.SellerId,
            CategoryId = g.CategoryId,
            Title = g.Title,
            Description = g.Description,
            Price = g.Price,
            Condition = MapCondition(g.GoodsCondition),
            Status = g.GoodsStatus,
            ViewCount = g.ViewCount,
            PublishTime = g.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            ImageUrl = g.CoverImage,
            SellerNickname = g.SellerName,
            CategoryName = g.CategoryName
        };
    }

    private static string MapCondition(string dbCondition)
    {
        return dbCondition switch
        {
            "全新" => "new",
            "几乎全新" => "like_new",
            "轻微使用" => "slight_use",
            "明显痕迹" => "obvious_trace",
            _ => "slight_use"
        };
    }
}
