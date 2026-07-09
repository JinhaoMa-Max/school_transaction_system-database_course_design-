using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 面交预约模块数据访问层接口（Repository 契约）
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// 根据预约主键获取预约详情
    /// </summary>
    /// <param name="appointmentId">预约 ID（appointment.appointment_id）</param>
    /// <returns>预约 DTO，若不存在则返回 null</returns>
    Task<AppointmentDto?> GetByIdAsync(int appointmentId);

    /// <summary>
    /// 根据订单主键获取预约信息（一单一约，appointment.order_id 唯一）
    /// </summary>
    /// <param name="orderId">订单 ID</param>
    /// <returns>预约 DTO，若不存在则返回 null</returns>
    /// <remarks>
    /// 字段映射：
    /// appointment_id -> AppointmentId，order_id -> OrderId，meet_time -> MeetTime，
    /// meet_place -> MeetLocation，confirm_code -> ConfirmCode，
    /// appointment_status -> Status，created_at -> CreateTime
    /// </remarks>
    Task<AppointmentDto?> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// 创建面交预约
    /// </summary>
    /// <param name="orderId">订单 ID</param>
    /// <param name="meetTime">面交时间</param>
    /// <param name="meetLocation">面交地点</param>
    /// <param name="confirmCode">确认码（6-10 位）</param>
    /// <returns>创建后的预约记录</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 插入 appointment 表，appointment_status 初始建议为 'pending'，created_at 使用 SYSTIMESTAMP。
    /// 2. appointment 表对 order_id 有唯一约束（uq_appointment_order_id），重复创建应返回明确错误。
    /// 3. confirm_code 建议由后端生成后写入，或由数据库侧统一生成（需与前端验证流程一致）。
    /// </remarks>
    Task<AppointmentDto> CreateAsync(int orderId, DateTime meetTime, string meetLocation, string confirmCode);

    /// <summary>
    /// 更新预约状态（pending/confirmed/completed/cancelled）
    /// </summary>
    /// <param name="appointmentId">预约 ID</param>
    /// <param name="status">目标状态</param>
    /// <returns>是否更新成功（影响行数 &gt; 0）</returns>
    Task<bool> UpdateStatusAsync(int appointmentId, string status);

    /// <summary>
    /// 校验确认码并完成面交（联动订单、商品、预约）
    /// </summary>
    /// <param name="orderId">订单 ID</param>
    /// <param name="confirmCode">确认码</param>
    /// <returns>是否处理成功</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 推荐直接调用存储过程 sp_complete_meet(p_order_id, p_confirm_code)。
    /// 2. 过程内应原子完成：trade_order.order_status='completed' + goods.goods_status='sold' + appointment.appointment_status='completed'。
    /// 3. F20 依赖订单完成状态：后续评价权限应以 order_status='completed' 为准做校验。
    /// </remarks>
    Task<bool> VerifyAndCompleteMeetAsync(int orderId, string confirmCode);
}
