using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IOrderService
{
    Task<OrderListResult> GetPagedAsync(int page, int size, string? status, int? currentUserId, string? role = null);
    Task<OrderDto?> GetByIdAsync(int orderId, int? currentUserId);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, int? currentUserId);
    Task<OrderDto> UpdateAsync(int orderId, UpdateOrderRequest request, int? currentUserId);
    Task<bool> CancelAsync(int orderId, int? currentUserId);
    Task<bool> StartMeetAsync(int orderId, int? currentUserId);
    Task<bool> CompleteAsync(int orderId, string? confirmCode, int? currentUserId);
}
