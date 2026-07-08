using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 议价数据访问层 — 仅此层进行数据库读写
/// </summary>
public class BargainRepository : IBargainRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BargainRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<(List<BargainOfferDto> Items, int Total)> GetPagedAsync(
        int page,
        int size,
        int? goodsId,
        int? buyerId,
        string? status)
    {
        throw new NotImplementedException("由数据库同学实现：分页查询议价列表（建议优先使用 v_active_bargains 视图）");
    }

    public Task<BargainOfferDto?> GetByIdAsync(int bargainId)
    {
        throw new NotImplementedException("由数据库同学实现：按 offer_id 查询议价详情");
    }

    public Task<BargainOfferDto> CreateAsync(int goodsId, int buyerId, decimal offerPrice)
    {
        throw new NotImplementedException("由数据库同学实现：创建议价（推荐调用 sp_create_bargain，并返回完整 BargainOfferDto）");
    }

    public Task<BargainOfferDto> RespondAsync(int bargainId, string sellerResult, decimal? counterPrice)
    {
        throw new NotImplementedException("由数据库同学实现：卖家回复议价（推荐调用 sp_respond_bargain，并返回更新后的 BargainOfferDto）");
    }

    public Task<BargainOfferDto> BuyerRespondAsync(int bargainId, string buyerResult, decimal? offerPrice)
    {
        throw new NotImplementedException("由数据库同学实现：买家回应议价（根据 buyerResult 更新 offer_status/offer_price/seller_response，buyerResult 不直接入库）");
    }

    public Task<bool> CloseAsync(int bargainId)
    {
        throw new NotImplementedException("由数据库同学实现：关闭议价（更新 offer_status 为 closed 或约定状态）");
    }
}
