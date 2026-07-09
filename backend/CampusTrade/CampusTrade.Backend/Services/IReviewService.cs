using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IReviewService
{
    Task<ApiResponse<ReviewListResult>> GetReviewsAsync(int page, int size, int? reviewerId, int? reviewedUserId, int? orderId);
    Task<ApiResponse<ReviewDto>> GetReviewByIdAsync(int reviewId);
    Task<ApiResponse<ReviewDto>> CreateReviewAsync(int orderId, int rating, string? content, int currentUserId);
    Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int reviewId, int rating, string? content, int currentUserId);
    Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId, int currentUserId);
}