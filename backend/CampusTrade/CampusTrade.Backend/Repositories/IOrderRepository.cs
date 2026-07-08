using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 订单 + 面交 Repository 接口（F15-F20）
/// 数据库: v_order_list 视图 / sp_place_order / sp_cancel_order / sp_complete_meet / fn_gen_confirm_code / fn_can_purchase
/// </summary>
public interface IOrderRepository
{
    // ==================== 查询 ====================
    Task<(List<OrderDto> Items, int Total)> GetPagedAsync(int page, int size, int? buyerId, int? sellerId, string? status);
    Task<OrderDto?> GetByIdAsync(int orderId);

    // ==================== 订单操作（F15-F17, F20）====================
    Task<int> CreateAsync(int goodsId, int buyerId, decimal price);       // sp_place_order
    Task<bool> CancelAsync(int orderId, int userId);                      // sp_cancel_order
    Task<bool> CompleteAsync(int orderId, string confirmCode);            // sp_complete_meet
    Task<bool> UpdateStatusAsync(int orderId, string newStatus);

    // ==================== 面交（F18-F19）====================
    Task<string> GenerateConfirmCodeAsync();                               // fn_gen_confirm_code
    Task<int> CreateAppointmentAsync(int orderId, DateTime meetTime, string meetPlace, string confirmCode);
    Task<AppointmentDto?> GetAppointmentByOrderIdAsync(int orderId);
    Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string newStatus);

    // ==================== 辅助 ====================
    Task<bool> IsParticipantAsync(int orderId, int userId);
    Task<bool> CanPurchaseAsync(int buyerId, int goodsId);                // fn_can_purchase
    Task<string?> GetStatusAsync(int orderId);
}
