using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class AdminService : IAdminService
{
    private static readonly HashSet<string> AuditTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "goods_audit",
        "report_handle",
        "user_ban",
        "goods_offline"
    };

    private static readonly HashSet<string> NoticeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "system",
        "transaction",
        "violation"
    };

    private readonly IAdminRepository _adminRepository;
    private readonly IUserRepository _userRepository;

    public AdminService(IAdminRepository adminRepository, IUserRepository userRepository)
    {
        _adminRepository = adminRepository;
        _userRepository = userRepository;
    }

    public async Task<int> RequireAdminAsync(int? currentUserId)
    {
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("login required");
        }

        var user = await _userRepository.GetByIdAsync(currentUserId.Value);
        if (user == null)
        {
            throw new UnauthorizedAccessException("login required");
        }

        if (string.Equals(user.Status, "banned", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("account is banned");
        }

        if (!string.Equals(user.Role, "admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("admin role required");
        }

        return currentUserId.Value;
    }

    public async Task<bool> IsAdminAsync(int? currentUserId)
    {
        if (!currentUserId.HasValue)
        {
            return false;
        }

        var user = await _userRepository.GetByIdAsync(currentUserId.Value);
        return user != null
            && !string.Equals(user.Status, "banned", StringComparison.OrdinalIgnoreCase)
            && string.Equals(user.Role, "admin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<AuditLogListResult> GetAuditLogsAsync(int? currentUserId, int page, int size, int? adminId, string? auditType)
    {
        await RequireAdminAsync(currentUserId);
        (page, size) = NormalizePaging(page, size);
        ValidateOptionalAuditType(auditType);

        var (items, total) = await _adminRepository.GetAuditLogsAsync(page, size, adminId, auditType);
        return new AuditLogListResult
        {
            List = items,
            Total = total,
            Page = page,
            Size = size
        };
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(int? currentUserId, int logId)
    {
        await RequireAdminAsync(currentUserId);
        return await _adminRepository.GetAuditLogByIdAsync(logId);
    }

    public async Task<AuditLogDto> CreateAuditLogAsync(int? currentUserId, CreateAuditLogRequest request)
    {
        var adminId = await RequireAdminAsync(currentUserId);
        ValidateAuditLogRequest(request);
        request.AuditType = request.AuditType.Trim();
        request.Action = request.Action.Trim();
        request.Result = string.IsNullOrWhiteSpace(request.Result) ? "success" : request.Result.Trim();
        request.Remark = string.IsNullOrWhiteSpace(request.Remark) ? null : request.Remark.Trim();
        return await _adminRepository.CreateAuditLogAsync(request, adminId);
    }

    public async Task<NoticeListResult> GetNoticesAsync(int? currentUserId, int page, int size, string? noticeType)
    {
        await RequireAdminAsync(currentUserId);
        (page, size) = NormalizePaging(page, size);
        ValidateOptionalNoticeType(noticeType);

        var (items, total) = await _adminRepository.GetNoticesAsync(page, size, noticeType);
        return new NoticeListResult
        {
            List = items,
            Total = total,
            Page = page,
            Size = size
        };
    }

    public async Task<NoticeDto?> GetNoticeByIdAsync(int? currentUserId, int noticeId)
    {
        await RequireAdminAsync(currentUserId);
        return await _adminRepository.GetNoticeByIdAsync(noticeId);
    }

    public async Task<NoticeDto> CreateNoticeAsync(int? currentUserId, CreateNoticeRequest request)
    {
        var adminId = await RequireAdminAsync(currentUserId);
        ValidateCreateNoticeRequest(request);
        request.Title = request.Title.Trim();
        request.Content = request.Content.Trim();
        request.NoticeType = request.NoticeType.Trim();
        return await _adminRepository.CreateNoticeAsync(request, adminId);
    }

    public async Task<NoticeDto> UpdateNoticeAsync(int? currentUserId, int noticeId, UpdateNoticeRequest request)
    {
        await RequireAdminAsync(currentUserId);
        ValidateUpdateNoticeRequest(request);
        var updated = await _adminRepository.UpdateNoticeAsync(noticeId, request);
        return updated ?? throw new InvalidOperationException("notice not found");
    }

    public async Task<bool> DeleteNoticeAsync(int? currentUserId, int noticeId)
    {
        await RequireAdminAsync(currentUserId);
        return await _adminRepository.DeleteNoticeAsync(noticeId);
    }

    private static (int Page, int Size) NormalizePaging(int page, int size)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);
        return (page, size);
    }

    private static void ValidateAuditLogRequest(CreateAuditLogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AuditType) || !AuditTypes.Contains(request.AuditType.Trim()))
        {
            throw new ArgumentException("invalid audit type");
        }

        if (request.TargetId <= 0)
        {
            throw new ArgumentException("targetId is required");
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            throw new ArgumentException("action is required");
        }
    }

    private static void ValidateOptionalAuditType(string? auditType)
    {
        if (!string.IsNullOrWhiteSpace(auditType) && !AuditTypes.Contains(auditType.Trim()))
        {
            throw new ArgumentException("invalid audit type");
        }
    }

    private static void ValidateCreateNoticeRequest(CreateNoticeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("title is required");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("content is required");
        }

        if (string.IsNullOrWhiteSpace(request.NoticeType) || !NoticeTypes.Contains(request.NoticeType.Trim()))
        {
            throw new ArgumentException("invalid notice type");
        }
    }

    private static void ValidateUpdateNoticeRequest(UpdateNoticeRequest request)
    {
        if (request.Title != null && string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("title cannot be empty");
        }

        if (request.Content != null && string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("content cannot be empty");
        }

        if (request.NoticeType != null && !NoticeTypes.Contains(request.NoticeType.Trim()))
        {
            throw new ArgumentException("invalid notice type");
        }
    }

    private static void ValidateOptionalNoticeType(string? noticeType)
    {
        if (!string.IsNullOrWhiteSpace(noticeType) && !NoticeTypes.Contains(noticeType.Trim()))
        {
            throw new ArgumentException("invalid notice type");
        }
    }
}