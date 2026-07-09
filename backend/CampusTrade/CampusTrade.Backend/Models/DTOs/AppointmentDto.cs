namespace CampusTrade.Backend.Models.DTOs;

// 预约记录（对齐前端 Appointment 模型）
public class AppointmentDto
{
    public int AppointmentId { get; set; }
    public int OrderId { get; set; }
    public DateTime MeetTime { get; set; }
    public string MeetLocation { get; set; } = string.Empty;
    public string ConfirmCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
}

// 创建预约入参：POST /api/appointments
public class CreateAppointmentRequest
{
    public int OrderId { get; set; }
    public DateTime MeetTime { get; set; }
    public string MeetLocation { get; set; } = string.Empty;
}

// 验证确认码入参：POST /api/appointments/verify
public class VerifyConfirmCodeRequest
{
    public int OrderId { get; set; }
    public string ConfirmCode { get; set; } = string.Empty;
}
