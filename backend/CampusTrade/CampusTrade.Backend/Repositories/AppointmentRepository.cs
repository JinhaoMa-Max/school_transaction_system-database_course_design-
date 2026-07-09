using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AppointmentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <remarks>
    /// 待补充实现要点：
    /// 1. 建议按 appointment_id 查询 appointment 表单条记录。
    /// 2. 字段映射：appointment_id->AppointmentId，order_id->OrderId，meet_time->MeetTime，
    ///    meet_place->MeetLocation，confirm_code->ConfirmCode，appointment_status->Status，created_at->CreateTime。
    /// </remarks>
    public Task<AppointmentDto?> GetByIdAsync(int appointmentId)
    {
        throw new NotImplementedException();
    }

    /// <remarks>
    /// 待补充实现要点：按 order_id 查询预约（appointment.order_id 唯一），字段映射同 GetByIdAsync。
    /// </remarks>
    public Task<AppointmentDto?> GetByOrderIdAsync(int orderId)
    {
        throw new NotImplementedException();
    }

    /// <remarks>
    /// 待补充实现要点：
    /// 1. INSERT appointment(order_id, meet_time, meet_place, confirm_code, appointment_status, created_at)。
    /// 2. 初始 appointment_status 建议为 'pending'，created_at 使用 SYSTIMESTAMP。
    /// 3. order_id 唯一约束冲突需返回明确错误（避免重复创建）。
    /// 4. 返回创建后的完整 AppointmentDto（供 Controller 直接返回前端）。
    /// </remarks>
    public Task<AppointmentDto> CreateAsync(int orderId, DateTime meetTime, string meetLocation, string confirmCode)
    {
        throw new NotImplementedException();
    }

    /// <remarks>
    /// 待补充实现要点：UPDATE appointment SET appointment_status=:status WHERE appointment_id=:appointmentId。
    /// </remarks>
    public Task<bool> UpdateStatusAsync(int appointmentId, string status)
    {
        throw new NotImplementedException();
    }

    /// <remarks>
    /// 待补充实现要点：
    /// 1. 推荐调用存储过程 sp_complete_meet(p_order_id, p_confirm_code)。
    /// 2. 过程内需原子完成：订单 completed、商品 sold、预约 completed（对应 F20 联动要求）。
    /// </remarks>
    public Task<bool> VerifyAndCompleteMeetAsync(int orderId, string confirmCode)
    {
        throw new NotImplementedException();
    }
}
