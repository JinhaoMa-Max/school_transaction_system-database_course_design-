using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// 分页查询订单列表
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 建议优先查询 v_order_list 视图。
    /// 2. 仅返回与 currentUserId 相关的订单（buyer_id=currentUserId 或 seller_id=currentUserId）。
    /// 3. 支持 status 可选过滤，并返回 (Items, Total)。
    /// </remarks>
    public Task<(List<TradeOrderDto> Items, int Total)> GetPagedAsync(int page, int size, string? status, int currentUserId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 查询订单详情
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 按 order_id 查询单条记录，不存在返回 null。
    /// 2. 字段映射：order_id->OrderId，final_price->DealPrice，order_status->Status，created_at->CreateTime。
    /// </remarks>
    public Task<TradeOrderDto?> GetByIdAsync(int orderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 推荐调用存储过程 sp_place_order（校验资格 + 锁商品 + 关闭议价 + 插入订单）。
    /// 2. F16“同一商品有效订单锁定判定”请数据库同学在过程/事务里原子实现，避免并发重复成交。
    /// 3. 下单取价依赖数据库约束：同一买家 + 同一商品仅保留一条议价记录，多轮议价在该记录上更新。
    /// 4. 若该议价最终成交，请保证 accepted 记录中的价格字段代表最后一次确认的成交价。
    /// 5. 返回创建后的完整 TradeOrderDto（供 Controller 直接返回前端）。
    /// </remarks>
    public Task<TradeOrderDto> CreateAsync(int goodsId, int buyerId, decimal dealPrice)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 更新订单（对齐前端 updateOrder）
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 建议只允许更新受控字段（如 order_status），并同步 updated_at。
    /// 2. 返回更新后的完整 TradeOrderDto。
    /// </remarks>
    public Task<TradeOrderDto> UpdateAsync(int orderId, UpdateOrderRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 取消订单
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 推荐调用 sp_cancel_order（恢复商品状态，取消预约）。
    /// 2. 返回是否更新成功（影响行数 > 0）。
    /// </remarks>
    public Task<bool> CancelAsync(int orderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 开始面交
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：更新 order_status 为 in_meet，并返回是否成功。
    /// </remarks>
    public Task<bool> StartMeetAsync(int orderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 完成交易
    /// </summary>
    /// <remarks>
    /// 待补充实现要点：
    /// 1. 推荐调用 sp_complete_meet（订单 completed，商品 sold）。
    /// 2. 返回是否更新成功（影响行数 > 0）。
    /// </remarks>
    public Task<bool> CompleteAsync(int orderId)
    {
        throw new NotImplementedException();
    }
}
