using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ApiResponse<ReportListResult>> GetReportsAsync(int page, int size, string? reportType, string? status)
    {
        var (items, total) = await _reportRepository.GetPagedAsync(page, size, reportType, status);
        return ApiResponse<ReportListResult>.Success(new ReportListResult
        {
            List = items,
            Total = total,
            Page = page,
            Size = size
        });
    }

    public async Task<ApiResponse<ReportDto>> GetReportByIdAsync(int reportId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            return ApiResponse<ReportDto>.Fail(404, "举报不存在");
        return ApiResponse<ReportDto>.Success(report);
    }

    public async Task<ApiResponse<ReportDto>> CreateReportAsync(CreateReportRequest request, int currentUserId)
    {
        if (string.IsNullOrWhiteSpace(request.ReportType))
            return ApiResponse<ReportDto>.Fail(400, "举报类型不能为空");

        var validTypes = new[] { "goods", "user", "order" };
        if (!validTypes.Contains(request.ReportType))
            return ApiResponse<ReportDto>.Fail(400, "举报类型无效，只能是 goods/user/order");

        var hasTarget = request.ReportedGoodsId.HasValue || request.ReportedUserId.HasValue || request.ReportedOrderId.HasValue;
        if (!hasTarget)
            return ApiResponse<ReportDto>.Fail(400, "必须指定举报目标");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return ApiResponse<ReportDto>.Fail(400, "举报理由不能为空");

        var reportId = await _reportRepository.CreateAsync(
            currentUserId,
            request.ReportType,
            request.ReportedGoodsId,
            request.ReportedUserId,
            request.ReportedOrderId,
            request.Reason);

        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            return ApiResponse<ReportDto>.Fail(500, "举报创建失败");

        return ApiResponse<ReportDto>.Success(report, "举报提交成功");
    }

    public async Task<ApiResponse<ReportDto>> HandleReportAsync(int reportId, HandleReportRequest request, int adminId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null)
            return ApiResponse<ReportDto>.Fail(404, "举报不存在");

        if (report.Status != "pending")
            return ApiResponse<ReportDto>.Fail(400, "该举报已处理");

        var validResults = new[] { "processing", "resolved", "rejected" };
        if (!validResults.Contains(request.Status))
            return ApiResponse<ReportDto>.Fail(400, "处理结果无效，只能是 processing/resolved/rejected");

        await _reportRepository.HandleAsync(reportId, adminId, request.Status, request.Remark);
        
        var updatedReport = await _reportRepository.GetByIdAsync(reportId);
        if (updatedReport == null)
            return ApiResponse<ReportDto>.Fail(500, "举报处理失败");
        
        return ApiResponse<ReportDto>.Success(updatedReport, "举报处理成功");
    }
}