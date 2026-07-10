using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 面交预约数据访问层（F18-F20）
/// 查询: appointment 表  事务: sp_complete_meet（原子完成：订单completed+商品sold+预约completed）
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public AppointmentRepository(IDbConnectionFactory connectionFactory) { _connectionFactory = connectionFactory; }

    public async Task<AppointmentDto?> GetByIdAsync(int appointmentId)
    {
        using var c = _connectionFactory.CreateConnection();
        return await c.QueryFirstOrDefaultAsync<AppointmentDto>("""
            SELECT appointment_id AS AppointmentId, order_id AS OrderId, meet_time AS MeetTime,
                   meet_place AS MeetLocation, confirm_code AS ConfirmCode,
                   appointment_status AS Status, created_at AS CreateTime
            FROM appointment WHERE appointment_id = :Id
            """, new { Id = appointmentId });
    }

    public async Task<AppointmentDto?> GetByOrderIdAsync(int orderId)
    {
        using var c = _connectionFactory.CreateConnection();
        return await c.QueryFirstOrDefaultAsync<AppointmentDto>("""
            SELECT appointment_id AS AppointmentId, order_id AS OrderId, meet_time AS MeetTime,
                   meet_place AS MeetLocation, confirm_code AS ConfirmCode,
                   appointment_status AS Status, created_at AS CreateTime
            FROM appointment WHERE order_id = :OrderId
            """, new { OrderId = orderId });
    }

    public async Task<AppointmentDto> CreateAsync(int orderId, DateTime meetTime, string meetLocation, string confirmCode)
    {
        using var c = _connectionFactory.CreateConnection();
        await c.ExecuteAsync("""
            INSERT INTO appointment (order_id, meet_time, meet_place, confirm_code, appointment_status, created_at)
            VALUES (:OrderId, :MeetTime, :MeetLocation, :Code, 'pending', SYSTIMESTAMP)
            """, new { OrderId = orderId, MeetTime = meetTime, MeetLocation = meetLocation, Code = confirmCode });
        return (await GetByOrderIdAsync(orderId))!;
    }

    public async Task<bool> UpdateStatusAsync(int appointmentId, string status)
    {
        using var c = _connectionFactory.CreateConnection();
        return await c.ExecuteAsync("UPDATE appointment SET appointment_status=:s WHERE appointment_id=:id",
            new { id = appointmentId, s = status }) > 0;
    }

    /// <summary>确认码核销+完成面交 — sp_complete_meet: 原子完成订单+商品+预约三联</summary>
    public async Task<bool> VerifyAndCompleteMeetAsync(int orderId, string confirmCode)
    {
        using var c = _connectionFactory.CreateConnection();
        if (c is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        await using var cmd = new OracleCommand("BEGIN sp_complete_meet(p_order_id=>:o, p_confirm_code=>:c); END;", oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("o", OracleDbType.Int32) { Value = orderId });
        cmd.Parameters.Add(new OracleParameter("c", OracleDbType.Varchar2, 10) { Value = confirmCode });
        await cmd.ExecuteNonQueryAsync();
        return true;
    }
}
