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

    public async Task<OrderListResult> GetPagedAsync(int page, int size, string? status, int? currentUserId, string? role = null)
    {
        var userId = RequireUser(currentUserId);
        ValidatePage(page, size);
        var normalizedStatus = NormalizeStatus(status, allowNull: true);
        var normalizedRole = role?.Trim().ToLowerInvariant();

        (List<OrderDto> Items, int Total) result = normalizedRole switch
        {
            "buy" or "buyer" => await _orderRepository.GetPagedAsync(page, size, buyerId: userId, sellerId: null, normalizedStatus),
            "sell" or "seller" => await _orderRepository.GetPagedAsync(page, size, buyerId: null, sellerId: userId, normalizedStatus),
            null or "" => await _orderRepository.GetPagedByParticipantAsync(page, size, userId, normalizedStatus),
            _ => throw new ArgumentException("role must be buy or sell")
        };

        return new OrderListResult
        {
            List = result.Items,
            Total = result.Total,
            Page = page,
            Size = size
        };
    }

    public async Task<OrderDto?> GetByIdAsync(int orderId, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        if (orderId <= 0) throw new ArgumentException("orderId is invalid");

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;
        EnsureParticipant(order, userId);
        return order;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, int? currentUserId)
    {
        var buyerId = RequireUser(currentUserId);
        if (request.GoodsId <= 0) throw new ArgumentException("goodsId is invalid");
        if (request.DealPrice <= 0) throw new ArgumentException("dealPrice must be greater than 0");

        var goods = await _goodsRepository.GetByIdAsync(request.GoodsId)
            ?? throw new ArgumentException("goods does not exist");
        if (goods.SellerId == buyerId) throw new InvalidOperationException("cannot buy your own goods");

        if (!await _orderRepository.CanPurchaseAsync(buyerId, request.GoodsId))
        {
            throw new InvalidOperationException("goods is not available for purchase");
        }

        var expectedPrice = await ResolveDealPriceAsync(request.GoodsId, buyerId, goods.Price);
        if (request.DealPrice != expectedPrice)
        {
            throw new InvalidOperationException("dealPrice does not match goods price or accepted bargain price");
        }

        var orderId = await _orderRepository.CreateAsync(request.GoodsId, buyerId, expectedPrice);
        return await _orderRepository.GetByIdAsync(orderId)
            ?? throw new InvalidOperationException("order was created but cannot be loaded");
    }

    public async Task<OrderDto> UpdateAsync(int orderId, UpdateOrderRequest request, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        if (orderId <= 0) throw new ArgumentException("orderId is invalid");
        if (request.DealPrice != null) throw new InvalidOperationException("dealPrice cannot be updated");

        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException("order does not exist");
        EnsureParticipant(order, userId);

        var status = NormalizeStatus(request.Status, allowNull: false)!;
        if (status == "completed")
        {
            throw new InvalidOperationException("use the complete endpoint to complete an order");
        }

        await _orderRepository.UpdateStatusAsync(orderId, status);
        return await _orderRepository.GetByIdAsync(orderId)
            ?? throw new InvalidOperationException("order was updated but cannot be loaded");
    }

    public async Task<bool> CancelAsync(int orderId, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        var order = await RequireExistingOrder(orderId, userId);
        if (order.Status != "pending_meet") throw new InvalidOperationException("only pending_meet orders can be cancelled");
        return await _orderRepository.CancelAsync(orderId, userId);
    }

    public async Task<bool> StartMeetAsync(int orderId, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        var order = await RequireExistingOrder(orderId, userId);
        if (order.Status != "pending_meet") throw new InvalidOperationException("only pending_meet orders can start meet");
        return await _orderRepository.UpdateStatusAsync(orderId, "in_meet");
    }

    public async Task<bool> CompleteAsync(int orderId, string? confirmCode, int? currentUserId)
    {
        var userId = RequireUser(currentUserId);
        var order = await RequireExistingOrder(orderId, userId);
        if (order.Status is not ("pending_meet" or "in_meet"))
        {
            throw new InvalidOperationException("only pending_meet or in_meet orders can be completed");
        }

        var code = string.IsNullOrWhiteSpace(confirmCode) ? order.ConfirmCode : confirmCode.Trim();
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("confirmCode is required");
        return await _orderRepository.CompleteAsync(orderId, code);
    }

    private async Task<OrderDto> RequireExistingOrder(int orderId, int userId)
    {
        if (orderId <= 0) throw new ArgumentException("orderId is invalid");
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException("order does not exist");
        EnsureParticipant(order, userId);
        return order;
    }

    private async Task<decimal> ResolveDealPriceAsync(int goodsId, int buyerId, decimal goodsPrice)
    {
        var (items, _) = await _bargainRepository.GetPagedAsync(
            page: 1,
            size: 1,
            goodsId: goodsId,
            buyerId: buyerId,
            sellerId: null,
            status: "accepted");

        var bargain = items.FirstOrDefault();
        if (bargain == null) return goodsPrice;
        return bargain.SellerResult == "countered" && bargain.CounterPrice != null
            ? bargain.CounterPrice.Value
            : bargain.OfferPrice;
    }

    private static void ValidatePage(int page, int size)
    {
        if (page < 1) throw new ArgumentException("page must start from 1");
        if (size < 1) throw new ArgumentException("size must be greater than 0");
    }

    private static int RequireUser(int? currentUserId)
    {
        return currentUserId ?? throw new UnauthorizedAccessException("login required");
    }

    private static void EnsureParticipant(OrderDto order, int userId)
    {
        if (order.BuyerId != userId && order.SellerId != userId)
        {
            throw new UnauthorizedAccessException("no permission for this order");
        }
    }

    private static string? NormalizeStatus(string? status, bool allowNull)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            if (allowNull) return null;
            throw new ArgumentException("status is required");
        }

        var normalized = status.Trim();
        return normalized is "pending_meet" or "in_meet" or "completed" or "cancelled"
            ? normalized
            : throw new ArgumentException("status is invalid");
    }
}
