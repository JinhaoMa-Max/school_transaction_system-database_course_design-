using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 议价模块数据访问层接口（Repository 契约）
/// </summary>
public interface IBargainRepository
{
    /// <summary>
    /// 分页查询议价列表
    /// </summary>
    /// <param name="page">页码（从 1 开始）</param>
    /// <param name="size">每页记录数</param>
    /// <param name="goodsId">商品 ID（可选，精确匹配）</param>
    /// <param name="buyerId">买家 ID（可选，精确匹配）</param>
    /// <param name="status">议价状态（可选，精确匹配，字段映射 bargain_offer.offer_status -> status）</param>
    /// <returns>元组：(议价列表 List&lt;BargainOfferDto&gt;, 总记录数 int)</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 建议优先查询视图 v_active_bargains（契约 docs/backend-rebuild-contract.md 指定）。
    /// 2. bargain_offer.offer_id 需映射为 BargainId；created_at 映射为 CreateTime。
    /// 3. Oracle 分页建议使用 OFFSET FETCH（12c+）或 ROWNUM 嵌套查询，并单独执行 COUNT(*)。
    /// 4. 建议默认按 CreateTime DESC 排序（如无明确需求）。
    /// 5. 返回字段需要完整映射为 DTO（字段映射见 docs/backend-rebuild-contract.md）：
    ///    offer_id -> BargainId，goods_id -> GoodsId，buyer_id -> BuyerId，
    ///    offer_price -> OfferPrice，seller_response -> SellerResult，
    ///    counter_price -> CounterPrice，offer_status -> Status，created_at -> CreateTime。
    /// 6. status/goodsId/buyerId 均为可选条件：传 null 时不加过滤。
    /// </remarks>
    Task<(List<BargainOfferDto> Items, int Total)> GetPagedAsync(
        int page,
        int size,
        int? goodsId,
        int? buyerId,
        string? status);

    /// <summary>
    /// 根据议价主键获取议价详情
    /// </summary>
    /// <param name="bargainId">议价 ID（字段映射 bargain_offer.offer_id）</param>
    /// <returns>议价 DTO，若不存在则返回 null</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 建议从 bargain_offer 表或对应详情视图查询。
    /// 2. seller_response 映射为 SellerResult；offer_status 映射为 Status。
    /// 3. 返回 DTO 字段需要齐全（同 GetPagedAsync 的字段映射要求）。
    /// </remarks>
    Task<BargainOfferDto?> GetByIdAsync(int bargainId);

    /// <summary>
    /// 创建议价（买家发起）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <param name="buyerId">买家用户 ID（从登录态获取）</param>
    /// <param name="offerPrice">出价金额</param>
    /// <returns>创建后的议价记录（用于直接返回给前端）</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 推荐调用存储过程 sp_create_bargain（见 docs/backend-rebuild-contract.md）。
    /// 2. created_at 使用 SYSDATE；初始 seller_response/offer_status 由数据库统一设定（避免业务层分歧）。
    /// 3. 若使用 INSERT，需使用 RETURNING offer_id INTO :变量 获取自增主键，并返回完整 DTO。
    /// 4. 需要返回的字段需齐全（BargainOfferDto 全字段），以便 Controller 直接返回给前端。
    /// </remarks>
    Task<BargainOfferDto> CreateAsync(int goodsId, int buyerId, decimal offerPrice);

    /// <summary>
    /// 卖家处理议价（接受/拒绝/还价）
    /// </summary>
    /// <param name="bargainId">议价 ID</param>
    /// <param name="sellerResult">卖家处理结果：accepted / rejected / countered</param>
    /// <param name="counterPrice">还价金额（当 sellerResult=countered 时需要写入）</param>
    /// <returns>处理后的议价记录</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 推荐调用存储过程 sp_respond_bargain（见 docs/backend-rebuild-contract.md）。
    /// 2. 需要写入 seller_response、counter_price，并根据业务规则更新 offer_status。
    /// 3. 可在数据库侧校验当前议价是否可被处理（例如：已关闭/已完成的不允许重复处理）。
    /// 4. 方法返回值需为更新后的完整记录（BargainOfferDto 全字段），供前端刷新展示。
    /// </remarks>
    Task<BargainOfferDto> RespondAsync(int bargainId, string sellerResult, decimal? counterPrice);

    /// <summary>
    /// 关闭议价（前端 closeBargain）
    /// </summary>
    /// <param name="bargainId">议价 ID</param>
    /// <returns>是否关闭成功（影响行数 &gt; 0）</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 建议更新 offer_status 为 'closed'（具体枚举由数据库同学最终确认）。
    /// 2. 仅更新状态与必要的更新时间字段（如 updated_at），避免覆盖其他信息。
    /// 3. 返回 bool：影响行数 &gt; 0 视为 true；无记录/未更新视为 false。
    /// </remarks>
    Task<bool> CloseAsync(int bargainId);
}
