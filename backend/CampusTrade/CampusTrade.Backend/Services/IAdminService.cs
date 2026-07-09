using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IAdminService
{
    Task<int> RequireAdminAsync(int? currentUserId);
    Task<bool> IsAdminAsync(int? currentUserId);

    Task<AuditLogListResult> GetAuditLogsAsync(int? currentUserId, int page, int size, int? adminId, string? auditType);
    Task<AuditLogDto?> GetAuditLogByIdAsync(int? currentUserId, int logId);
    Task<AuditLogDto> CreateAuditLogAsync(int? currentUserId, CreateAuditLogRequest request);

    Task<NoticeListResult> GetNoticesAsync(int? currentUserId, int page, int size, string? noticeType);
    Task<NoticeDto?> GetNoticeByIdAsync(int? currentUserId, int noticeId);
    Task<NoticeDto> CreateNoticeAsync(int? currentUserId, CreateNoticeRequest request);
    Task<NoticeDto> UpdateNoticeAsync(int? currentUserId, int noticeId, UpdateNoticeRequest request);
    Task<bool> DeleteNoticeAsync(int? currentUserId, int noticeId);
}