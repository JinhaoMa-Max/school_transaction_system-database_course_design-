using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

public interface IAdminRepository
{
    Task<(List<AuditLogDto> Items, int Total)> GetAuditLogsAsync(int page, int size, int? adminId, string? auditType);
    Task<AuditLogDto?> GetAuditLogByIdAsync(int logId);
    Task<AuditLogDto> CreateAuditLogAsync(CreateAuditLogRequest request, int adminId);

    Task<AdminStatsDto> GetStatsAsync();

    Task<(List<NoticeDto> Items, int Total)> GetNoticesAsync(int page, int size, string? noticeType);
    Task<NoticeDto?> GetNoticeByIdAsync(int noticeId);
    Task<NoticeDto> CreateNoticeAsync(CreateNoticeRequest request, int publisherId);
    Task<NoticeDto?> UpdateNoticeAsync(int noticeId, UpdateNoticeRequest request);
    Task<bool> DeleteNoticeAsync(int noticeId);
}