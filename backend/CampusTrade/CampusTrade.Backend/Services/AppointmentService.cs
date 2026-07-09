using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;
using System.Security.Cryptography;

namespace CampusTrade.Backend.Services;

/// <summary>
/// 面交预约业务层：处理权限校验、状态流转与 F20 联动入口
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IOrderRepository _orderRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository, IOrderRepository orderRepository)
    {
        _appointmentRepository = appointmentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<AppointmentDto?> GetByOrderIdAsync(int orderId, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (orderId <= 0) throw new ArgumentException("orderId 不合法");

        // 预约依附订单存在，仅买卖双方可查看
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) throw new ArgumentException("订单不存在");

        if (order.BuyerId != currentUserId.Value && order.SellerId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException("无权限查看该预约");
        }

        return await _appointmentRepository.GetByOrderIdAsync(orderId);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (request.OrderId <= 0) throw new ArgumentException("orderId 不合法");
        if (string.IsNullOrWhiteSpace(request.MeetLocation)) throw new ArgumentException("meetLocation 不能为空");

        var meetLocation = request.MeetLocation.Trim();

        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) throw new ArgumentException("订单不存在");

        var uid = currentUserId.Value;
        if (order.BuyerId != uid && order.SellerId != uid)
        {
            throw new UnauthorizedAccessException("无权限创建该预约");
        }

        if (order.Status != "pending_meet" && order.Status != "in_meet")
        {
            throw new InvalidOperationException("当前订单状态不允许创建预约");
        }

        // 一单一约：已存在预约时不允许重复创建
        var existing = await _appointmentRepository.GetByOrderIdAsync(request.OrderId);
        if (existing != null) throw new InvalidOperationException("该订单已存在预约");

        var confirmCode = GenerateConfirmCode();
        return await _appointmentRepository.CreateAsync(request.OrderId, request.MeetTime, meetLocation, confirmCode);
    }

    public async Task<bool> ConfirmAsync(int appointmentId, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (appointmentId <= 0) throw new ArgumentException("appointmentId 不合法");

        var appt = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appt == null) return false;

        var order = await _orderRepository.GetByIdAsync(appt.OrderId);
        if (order == null) throw new ArgumentException("订单不存在");

        var uid = currentUserId.Value;
        if (order.BuyerId != uid && order.SellerId != uid)
        {
            throw new UnauthorizedAccessException("无权限确认该预约");
        }

        // 预约确认仅改变预约状态，不在此处联动订单/商品
        if (appt.Status != "pending")
        {
            throw new InvalidOperationException("仅待确认预约可确认");
        }

        return await _appointmentRepository.UpdateStatusAsync(appointmentId, "confirmed");
    }

    public async Task<bool> CancelAsync(int appointmentId, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (appointmentId <= 0) throw new ArgumentException("appointmentId 不合法");

        var appt = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appt == null) return false;

        var order = await _orderRepository.GetByIdAsync(appt.OrderId);
        if (order == null) throw new ArgumentException("订单不存在");

        var uid = currentUserId.Value;
        if (order.BuyerId != uid && order.SellerId != uid)
        {
            throw new UnauthorizedAccessException("无权限取消该预约");
        }

        if (appt.Status == "completed" || appt.Status == "cancelled")
        {
            throw new InvalidOperationException("预约已结束，无法取消");
        }

        return await _appointmentRepository.UpdateStatusAsync(appointmentId, "cancelled");
    }

    public async Task<bool> CompleteAsync(int appointmentId, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (appointmentId <= 0) throw new ArgumentException("appointmentId 不合法");

        var appt = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appt == null) return false;

        var order = await _orderRepository.GetByIdAsync(appt.OrderId);
        if (order == null) throw new ArgumentException("订单不存在");

        var uid = currentUserId.Value;
        if (order.BuyerId != uid && order.SellerId != uid)
        {
            throw new UnauthorizedAccessException("无权限完成该预约");
        }

        // F20 的核心联动入口是 verify，不是这个 complete 接口本身
        if (order.Status != "completed")
        {
            throw new InvalidOperationException("请使用确认码验证完成交易");
        }

        if (appt.Status == "completed") return true;

        return await _appointmentRepository.UpdateStatusAsync(appointmentId, "completed");
    }

    public async Task<bool> VerifyConfirmCodeAsync(VerifyConfirmCodeRequest request, int? currentUserId)
    {
        if (currentUserId == null) throw new UnauthorizedAccessException("未登录");
        if (request.OrderId <= 0) throw new ArgumentException("orderId 不合法");
        if (string.IsNullOrWhiteSpace(request.ConfirmCode)) throw new ArgumentException("confirmCode 不能为空");

        var confirmCode = request.ConfirmCode.Trim();

        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) throw new ArgumentException("订单不存在");

        var uid = currentUserId.Value;
        if (order.BuyerId != uid && order.SellerId != uid)
        {
            throw new UnauthorizedAccessException("无权限验证确认码");
        }

        // 确认码验证成功后，Repository/数据库过程需原子联动订单 completed、商品 sold、预约 completed
        return await _appointmentRepository.VerifyAndCompleteMeetAsync(request.OrderId, confirmCode);
    }

    private static string GenerateConfirmCode()
    {
        const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var length = RandomNumberGenerator.GetInt32(6, 11);
        var buffer = new char[length];
        for (var i = 0; i < length; i++)
        {
            buffer[i] = charset[RandomNumberGenerator.GetInt32(charset.Length)];
        }

        return new string(buffer);
    }
}
