using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 举报模块 Repository 接口（F22-F23）
/// 数据库: report 表 / sp_handle_report / trg_report_threshold_alert（触发器自动）
/// </summary>
public interface IReportRepository
{
    /// <summary>举报列表</summary>
    Task<(List<ReportDto> Items, int Total)> GetPagedAsync(int page, int size, string? reportType, string? status);

    /// <summary>举报详情</summary>
    Task<ReportDto?> GetByIdAsync(int reportId);

    /// <summary>提交举报 — 触发器 trg_report_threshold_alert 自动检测阈值</summary>
    Task<int> CreateAsync(int reporterId, string reportType, int? targetGoodsId, int? targetUserId, int? targetOrderId, string reason);

    /// <summary>处理举报（管理员）— 调用 sp_handle_report</summary>
    Task<bool> HandleAsync(int reportId, int adminId, string result, string? remark);

    /// <summary>更新举报状态</summary>
    Task<bool> UpdateStatusAsync(int reportId, string newStatus);

    /// <summary>校验举报目标是否存在</summary>
    Task<bool> TargetExistsAsync(string reportType, int targetId);
}
