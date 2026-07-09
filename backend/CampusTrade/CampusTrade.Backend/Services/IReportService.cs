using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IReportService
{
    Task<ApiResponse<ReportListResult>> GetReportsAsync(int page, int size, string? reportType, string? status);
    Task<ApiResponse<ReportDto>> GetReportByIdAsync(int reportId);
    Task<ApiResponse<ReportDto>> CreateReportAsync(CreateReportRequest request, int currentUserId);
    Task<ApiResponse<ReportDto>> HandleReportAsync(int reportId, HandleReportRequest request, int adminId);
}