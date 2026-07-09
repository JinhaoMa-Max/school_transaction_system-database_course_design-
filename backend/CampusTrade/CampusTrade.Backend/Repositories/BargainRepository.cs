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

    /// <summary>
    /// 分页查询议价列表
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 建议优先使用 v_active_bargains 视图（若无则查询 bargain_offer 表）。
    /// 2. 支持 goodsId/buyerId/status 条件过滤，并返回 (Items, Total)。
    /// 3. 字段映射：offer_id->BargainId，seller_response->SellerResult，offer_status->Status，created_at->CreateTime。
    /// </remarks>
    public Task<(List<BargainOfferDto> Items, int Total)> GetPagedAsync(
        int page,
        int size,
        int? goodsId,
        int? buyerId,
        string? status)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 根据议价 ID 获取详情
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 按 offer_id 查询单条记录，不存在返回 null。
    /// 2. 字段映射同上。
    /// </remarks>
    public Task<BargainOfferDto?> GetByIdAsync(int bargainId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 创建议价（买家发起）
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 推荐调用存储过程 sp_create_bargain，生成 offer_id 并写入 created_at、初始状态等。
    /// 2. 请数据库同学保证同一买家 + 同一商品仅保留一条议价记录；若已存在，则应拒绝重复创建或转为更新原记录。
    /// 3. 返回创建后的完整 BargainOfferDto（供 Controller 直接返回前端）。
    /// </remarks>
    public Task<BargainOfferDto> CreateAsync(int goodsId, int buyerId, decimal offerPrice)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 卖家回复议价（接受/拒绝/还价）
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 推荐调用存储过程 sp_respond_bargain，写入 seller_response、counter_price，并更新 offer_status。
    /// 2. 多轮议价请始终更新同一条议价记录：卖家还价时更新 counter_price，后续成交仍基于该记录继续流转。
    /// 3. 返回更新后的完整 BargainOfferDto。
    /// </remarks>
    public Task<BargainOfferDto> RespondAsync(int bargainId, string sellerResult, decimal? counterPrice)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 关闭议价
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 更新 offer_status 为 'closed'（或你们最终约定的关闭状态值）。
    /// 2. 返回是否更新成功（影响行数 &gt; 0）。
    /// </remarks>
    public Task<bool> CloseAsync(int bargainId)
    {
        throw new NotImplementedException();
    }
}
