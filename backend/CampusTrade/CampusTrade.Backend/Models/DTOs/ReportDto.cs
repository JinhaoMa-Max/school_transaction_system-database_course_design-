namespace CampusTrade.Backend.Models.DTOs;

/// <summary>举报 DTO — 对应 report 表</summary>
public class ReportDto
{
    public int ReportId { get; set; }
    public int ReporterId { get; set; }
    public string ReportType { get; set; } = string.Empty;   // 'goods' | 'user' | 'order'
    public int? TargetGoodsId { get; set; }
    public int? TargetUserId { get; set; }
    public int? TargetOrderId { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;       // report_status → 前端 status
    public DateTime CreateTime { get; set; }                 // created_at → 前端 reportTime
}

public class ReportListResult { public List<ReportDto> List { get; set; } = new(); public int Total { get; set; } public int Page { get; set; } public int Size { get; set; } }

/// <summary>提交举报请求</summary>
public class CreateReportRequest { public string ReportType { get; set; } = string.Empty; public int? TargetGoodsId { get; set; } public int? TargetUserId { get; set; } public int? TargetOrderId { get; set; } public string Reason { get; set; } = string.Empty; }

/// <summary>处理举报请求（管理员）</summary>
public class HandleReportRequest { public string Result { get; set; } = string.Empty; public string? Remark { get; set; } }
