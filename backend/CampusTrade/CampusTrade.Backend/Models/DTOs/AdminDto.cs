namespace CampusTrade.Backend.Models.DTOs;

public class AuditLogDto
{
    public int LogId { get; set; }
    public int AdminId { get; set; }
    public string AuditType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public DateTime HandleTime { get; set; }
}

public class AuditLogListResult
{
    public List<AuditLogDto> List { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

public class CreateAuditLogRequest
{
    public int? AdminId { get; set; }
    public string AuditType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? Remark { get; set; }
}

public class NoticeDto
{
    public int NoticeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string NoticeType { get; set; } = string.Empty;
    public int PublisherId { get; set; }
    public DateTime PublishTime { get; set; }
}

public class NoticeListResult
{
    public List<NoticeDto> List { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

public class CreateNoticeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string NoticeType { get; set; } = string.Empty;
}

public class UpdateNoticeRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? NoticeType { get; set; }
}

public class AdminStatsDto
{
    public int UserCount { get; set; }
    public int GoodsCount { get; set; }
    public int OrderCount { get; set; }
    public int ReportCount { get; set; }
}