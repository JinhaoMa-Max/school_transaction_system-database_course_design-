using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

public interface IOrderRepository
{
    Task<(List<OrderDto> Items, int Total)> GetPagedAsync(int page, int size, int? buyerId, int? sellerId, string? status);
    Task<(List<OrderDto> Items, int Total)> GetPagedByParticipantAsync(int page, int size, int userId, string? status);
    Task<OrderDto?> GetByIdAsync(int orderId);

    Task<int> CreateAsync(int goodsId, int buyerId, decimal price);
    Task<bool> CancelAsync(int orderId, int userId);
    Task<bool> CompleteAsync(int orderId, string confirmCode);
    Task<bool> UpdateStatusAsync(int orderId, string newStatus);

    Task<string> GenerateConfirmCodeAsync();
    Task<int> CreateAppointmentAsync(int orderId, DateTime meetTime, string meetLocation, string confirmCode);
    Task<AppointmentDto?> GetAppointmentByOrderIdAsync(int orderId);
    Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string newStatus);

    Task<bool> IsParticipantAsync(int orderId, int userId);
    Task<bool> CanPurchaseAsync(int buyerId, int goodsId);
    Task<string?> GetStatusAsync(int orderId);
}
