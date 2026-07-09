using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 订单模块数据访问层接口（Repository 契约）
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// 分页查询订单列表（仅返回与当前用户相关的订单：buyer_id 或 seller_id 匹配）
    /// </summary>
    /// <param name="page">页码（从 1 开始）</param>
    /// <param name="size">每页记录数</param>
    /// <param name="status">订单状态（可选，字段映射 trade_order.order_status -> status）</param>
    /// <param name="currentUserId">当前用户 ID（用于过滤 buyer/seller）</param>
    /// <returns>元组：(订单列表 List&lt;TradeOrderDto&gt;, 总记录数 int)</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 建议优先查询视图 v_order_list（契约建议）。
    /// 2. Oracle 分页建议使用 OFFSET FETCH（12c+）或 ROWNUM，并单独 COUNT(*)。
    /// 3. 字段映射：order_id->OrderId，goods_id->GoodsId，buyer_id->BuyerId，seller_id->SellerId，
    ///    final_price->DealPrice，order_status->Status，created_at->CreateTime。
    /// </remarks>
    Task<(List<TradeOrderDto> Items, int Total)> GetPagedAsync(int page, int size, string? status, int currentUserId);

    /// <summary>
    /// 根据订单主键获取订单详情
    /// </summary>
    /// <param name="orderId">订单 ID（trade_order.order_id）</param>
    /// <returns>订单 DTO，若不存在则返回 null</returns>
    Task<TradeOrderDto?> GetByIdAsync(int orderId);

    /// <summary>
    /// 创建订单（推荐使用存储过程 sp_place_order：校验购买资格 + 锁商品 + 关闭议价 + 插入订单）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <param name="buyerId">买家用户 ID</param>
    /// <param name="dealPrice">成交价（最终写入 trade_order.final_price）</param>
    /// <returns>创建后的订单记录（用于直接返回前端）</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 推荐调用 sp_place_order(p_goods_id, p_buyer_id, p_price, p_order_id) 并返回新订单详情。
    /// 2. F16“同一商品有效订单锁定判定”必须由数据库同学在过程内原子实现，避免并发下重复成交。
    /// 3. 下单取价依赖数据库约束：同一买家 + 同一商品仅保留一条议价记录，多轮议价在该记录上更新。
    /// 4. 若该议价最终成交，过程内返回/落库时需保证 accepted 记录中的价格字段代表最终成交价。
    /// 5. 过程内建议完成：goods 状态从 approved -> locked，关闭该商品相关的 active 议价记录。
    /// </remarks>
    Task<TradeOrderDto> CreateAsync(int goodsId, int buyerId, decimal dealPrice);

    /// <summary>
    /// 更新订单（用于对齐前端 updateOrder：Partial&lt;TradeOrder&gt;）
    /// </summary>
    /// <param name="orderId">订单 ID</param>
    /// <param name="request">允许更新的字段（建议只允许更新状态等受控字段）</param>
    /// <returns>更新后的订单记录（用于直接返回前端）</returns>
    Task<TradeOrderDto> UpdateAsync(int orderId, UpdateOrderRequest request);

    /// <summary>
    /// 取消订单（推荐存储过程 sp_cancel_order：恢复商品，取消预约）
    /// </summary>
    /// <param name="orderId">订单 ID</param>
    /// <returns>是否取消成功</returns>
    Task<bool> CancelAsync(int orderId);

    /// <summary>
    /// 开始面交（状态 pending_meet -> in_meet）
    /// </summary>
    /// <param name="orderId">订单 ID</param>
    /// <returns>是否更新成功</returns>
    Task<bool> StartMeetAsync(int orderId);

    /// <summary>
    /// 完成面交（推荐存储过程 sp_complete_meet：订单 completed，商品 sold）
    /// </summary>
    /// <param name="orderId">订单 ID</param>
    /// <returns>是否完成成功</returns>
    Task<bool> CompleteAsync(int orderId);
}
