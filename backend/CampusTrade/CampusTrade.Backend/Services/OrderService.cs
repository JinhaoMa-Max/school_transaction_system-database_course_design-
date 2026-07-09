using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IGoodsRepository _goodsRepository;
    private readonly IBargainRepository _bargainRepository;

    public OrderService(IOrderRepository orderRepository, IGoodsRepository goodsRepository, IBargainRepository bargainRepository)
    {
        _orderRepository = orderRepository;
        _goodsRepository = goodsRepository;
        _bargainRepository = bargainRepository;
    }

    public async Task<OrderListResult> GetPagedAsync(int page, int size, string? status, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (page < 1) throw new ArgumentException("page 必须从 1 开始");
        if (size < 1) throw new ArgumentException("size 必须大于 0");

        var (items, total) = await _orderRepository.GetPagedAsync(page, size, status, currentUserId.Value);
        return new OrderListResult
        {
            List = items,
            Total = total,
            Page = page,
            Size = size
        };
    }

    public async Task<TradeOrderDto?> GetByIdAsync(int orderId, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (orderId <= 0) throw new ArgumentException("orderId 不合法");

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;

        if (order.BuyerId != currentUserId.Value && order.SellerId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException("无权限查看该订单");
        }

        return order;
    }

    public async Task<TradeOrderDto> CreateAsync(CreateOrderRequest request, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (request.GoodsId <= 0) throw new ArgumentException("goodsId 不合法");
        if (request.DealPrice <= 0) throw new ArgumentException("dealPrice 不合法");

        var buyerId = currentUserId.Value;

        var goods = await _goodsRepository.GetByIdAsync(request.GoodsId);
        if (goods == null) throw new ArgumentException("商品不存在");
        if (goods.SellerId == buyerId) throw new InvalidOperationException("不能购买自己发布的商品");

        // 这里只做业务前置校验；F16“同一商品有效订单锁定判定”必须由数据库同学
        // 在下单存储过程/事务里原子完成（锁商品 + 创建订单），仅靠这里的查询无法防并发重复成交。
        var available = await _goodsRepository.IsAvailableForPurchaseAsync(request.GoodsId);
        if (!available) throw new InvalidOperationException("商品当前不可购买");

        var expectedPrice = await ResolveDealPriceAsync(request.GoodsId, buyerId, goods.Price);
        if (request.DealPrice != expectedPrice)
        {
            throw new InvalidOperationException("成交价不匹配议价结果或商品标价");
        }

        return await _orderRepository.CreateAsync(request.GoodsId, buyerId, expectedPrice);
    }

    public async Task<TradeOrderDto> UpdateAsync(int orderId, UpdateOrderRequest request, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (orderId <= 0) throw new ArgumentException("orderId 不合法");

        var existing = await _orderRepository.GetByIdAsync(orderId);
        if (existing == null) throw new ArgumentException("订单不存在");

        if (existing.BuyerId != currentUserId.Value && existing.SellerId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException("无权限更新该订单");
        }

        if (request.DealPrice != null)
        {
            throw new InvalidOperationException("成交价不允许通过该接口修改");
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new ArgumentException("status 不能为空");
        }

        var status = request.Status.Trim();
        if (status != "pending_meet" && status != "in_meet" && status != "completed" && status != "cancelled")
        {
            throw new ArgumentException("status 不合法");
        }

        return await _orderRepository.UpdateAsync(orderId, new UpdateOrderRequest { Status = status });
    }

    public async Task<bool> CancelAsync(int orderId, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (orderId <= 0) throw new ArgumentException("orderId 不合法");

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        if (order.BuyerId != currentUserId.Value && order.SellerId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException("无权限取消该订单");
        }

        if (order.Status == "completed" || order.Status == "cancelled")
        {
            throw new InvalidOperationException("订单已结束，无法取消");
        }

        return await _orderRepository.CancelAsync(orderId);
    }

    public async Task<bool> StartMeetAsync(int orderId, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (orderId <= 0) throw new ArgumentException("orderId 不合法");

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        if (order.BuyerId != currentUserId.Value && order.SellerId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException("无权限操作该订单");
        }

        if (order.Status != "pending_meet")
        {
            throw new InvalidOperationException("仅待面交订单可开始面交");
        }

        return await _orderRepository.StartMeetAsync(orderId);
    }

    public async Task<bool> CompleteAsync(int orderId, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (orderId <= 0) throw new ArgumentException("orderId 不合法");

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        if (order.BuyerId != currentUserId.Value && order.SellerId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException("无权限操作该订单");
        }

        if (order.Status != "in_meet")
        {
            throw new InvalidOperationException("仅面交中订单可完成交易");
        }

        return await _orderRepository.CompleteAsync(orderId);
    }

    private async Task<decimal> ResolveDealPriceAsync(int goodsId, int buyerId, decimal goodsPrice)
    {
        // 依赖数据库约束：同一买家 + 同一商品（商品唯一对应卖家）仅保留一条有效议价记录，
        // 多轮议价必须在该记录上持续更新；下单时读取这条最终 status=accepted 的记录即可。
        var (items, _) = await _bargainRepository.GetPagedAsync(
            page: 1,
            size: 1,
            goodsId: goodsId,
            buyerId: buyerId,
            status: "accepted");

        var bargain = items.FirstOrDefault();
        if (bargain == null) return goodsPrice;

        if (bargain.SellerResult == "countered" && bargain.CounterPrice != null)
        {
            return bargain.CounterPrice.Value;
        }

        return bargain.OfferPrice;
    }
}
