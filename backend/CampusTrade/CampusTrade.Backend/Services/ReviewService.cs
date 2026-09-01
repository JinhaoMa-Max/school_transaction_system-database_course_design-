using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<ApiResponse<ReviewListResult>> GetReviewsAsync(int page, int size, int? reviewerId, int? reviewedUserId, int? orderId)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);
        var (items, total) = await _reviewRepository.GetPagedAsync(page, size, reviewerId, reviewedUserId, orderId);
        return ApiResponse<ReviewListResult>.Success(new ReviewListResult
        {
            List = items,
            Total = total,
            Page = page,
            Size = size
        });
    }

    public async Task<ApiResponse<ReviewDto>> GetReviewByIdAsync(int reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return ApiResponse<ReviewDto>.Fail(404, "评价不存在");
        return ApiResponse<ReviewDto>.Success(review);
    }

    public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(int orderId, int reviewedUserId, int rating, string? content, int currentUserId)
    {
        if (rating < 1 || rating > 5)
            return ApiResponse<ReviewDto>.Fail(400, "评分必须在1-5之间");
        content = content?.Trim();
        if (content?.Length > 1000)
            return ApiResponse<ReviewDto>.Fail(400, "评价内容不能超过 1000 个字符");

        // 检查是否已达上限（首评+追评各一次，共2条）
        var reviewCount = await _reviewRepository.GetReviewCountAsync(orderId, currentUserId);
        if (reviewCount >= 2)
            return ApiResponse<ReviewDto>.Fail(409, "您已达到评价次数上限（首评+追评各一次）");

        // 追评时检查7天期限
        if (reviewCount == 1)
        {
            var firstReviewDays = await _reviewRepository.GetFirstReviewDaysAsync(orderId, currentUserId);
            if (firstReviewDays > 7)
                return ApiResponse<ReviewDto>.Fail(409, "追评已超过7天期限，无法追评");
        }

        var reviewId = await _reviewRepository.CreateAsync(orderId, currentUserId, reviewedUserId, rating, content);
        var review = await _reviewRepository.GetByIdAsync(reviewId);

        if (review == null)
            return ApiResponse<ReviewDto>.Fail(500, "评价创建失败");

        var msg = reviewCount == 0 ? "评价成功" : "追评成功";
        return ApiResponse<ReviewDto>.Success(review, msg);
    }

    public async Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int reviewId, int rating, string? content, int currentUserId)
    {
        if (rating < 1 || rating > 5)
            return ApiResponse<ReviewDto>.Fail(400, "评分必须在1-5之间");
        content = content?.Trim();
        if (content?.Length > 1000)
            return ApiResponse<ReviewDto>.Fail(400, "评价内容不能超过 1000 个字符");

        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return ApiResponse<ReviewDto>.Fail(404, "评价不存在");

        if (review.ReviewerId != currentUserId)
            return ApiResponse<ReviewDto>.Fail(403, "无权修改该评价");

        var success = await _reviewRepository.UpdateAsync(reviewId, rating, content);
        if (!success)
            return ApiResponse<ReviewDto>.Fail(500, "评价更新失败");

        await _reviewRepository.RecalcCreditAsync(review.ReviewedUserId);

        var updatedReview = await _reviewRepository.GetByIdAsync(reviewId);
        return ApiResponse<ReviewDto>.Success(updatedReview!, "评价更新成功");
    }

    public async Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId, int currentUserId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return ApiResponse<bool>.Fail(404, "评价不存在");

        if (review.ReviewerId != currentUserId)
            return ApiResponse<bool>.Fail(403, "无权删除该评价");

        var success = await _reviewRepository.DeleteAsync(reviewId);
        if (!success)
            return ApiResponse<bool>.Fail(500, "评价删除失败");

        await _reviewRepository.RecalcCreditAsync(review.ReviewedUserId);

        return ApiResponse<bool>.Success(true, "评价删除成功");
    }
}
