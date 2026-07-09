using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

/// <summary>
/// 面交预约业务层接口
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// 根据订单获取预约信息
    /// </summary>
    Task<AppointmentDto?> GetByOrderIdAsync(int orderId, int? currentUserId);

    /// <summary>
    /// 创建面交预约
    /// </summary>
    Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, int? currentUserId);

    /// <summary>
    /// 确认预约
    /// </summary>
    Task<bool> ConfirmAsync(int appointmentId, int? currentUserId);

    /// <summary>
    /// 取消预约
    /// </summary>
    Task<bool> CancelAsync(int appointmentId, int? currentUserId);

    /// <summary>
    /// 完成预约
    /// </summary>
    Task<bool> CompleteAsync(int appointmentId, int? currentUserId);

    /// <summary>
    /// 验证确认码并联动完成交易
    /// </summary>
    Task<bool> VerifyConfirmCodeAsync(VerifyConfirmCodeRequest request, int? currentUserId);
}
