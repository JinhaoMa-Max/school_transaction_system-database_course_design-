using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IOrderService
{
    Task<OrderListResult> GetPagedAsync(int page, int size, string? status, int? currentUserId);

    Task<TradeOrderDto?> GetByIdAsync(int orderId, int? currentUserId);

    Task<TradeOrderDto> CreateAsync(CreateOrderRequest request, int? currentUserId);

    Task<TradeOrderDto> UpdateAsync(int orderId, UpdateOrderRequest request, int? currentUserId);

    Task<bool> CancelAsync(int orderId, int? currentUserId);

    Task<bool> StartMeetAsync(int orderId, int? currentUserId);

    Task<bool> CompleteAsync(int orderId, int? currentUserId);
}

