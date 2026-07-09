using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 评价模块 Repository 接口（F21）
/// 数据库: v_review_detail 视图 / sp_create_review / fn_calc_credit / fn_avg_rating
/// </summary>
public interface IReviewRepository
{
    /// <summary>评价列表（支持按评价者/被评价者/订单筛选）</summary>
Task<(List<ReviewDto> Items, int Total)> GetPagedAsync(int page, int size, int? reviewerId, int? reviewedUserId, int? orderId);

    /// <summary>评价详情</summary>
    Task<ReviewDto?> GetByIdAsync(int reviewId);

    /// <summary>创建评价 — 调用 sp_create_review（校验已完成、不重复、自动更新信用分）</summary>
    Task<int> CreateAsync(int orderId, int reviewerId, int rating, string? content);

    /// <summary>检查某人对某订单是否已评价</summary>
    Task<bool> HasReviewedAsync(int orderId, int reviewerId);

    /// <summary>用户平均评分 — 调用 fn_avg_rating</summary>
    Task<decimal> GetAvgRatingAsync(int userId);

    /// <summary>重算信用分 — 调用 fn_calc_credit</summary>
Task<int> RecalcCreditAsync(int userId);

/// <summary>更新评价</summary>
Task<bool> UpdateAsync(int reviewId, int rating, string? content);

/// <summary>删除评价</summary>
Task<bool> DeleteAsync(int reviewId);
}
