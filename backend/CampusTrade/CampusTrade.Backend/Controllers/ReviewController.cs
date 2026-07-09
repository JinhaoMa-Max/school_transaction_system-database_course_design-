using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly IAuthService _authService;

    public ReviewController(IReviewService reviewService, IAuthService authService)
    {
        _reviewService = reviewService;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReviews([FromQuery] int page = 1, [FromQuery] int size = 20, [FromQuery] int? reviewerId = null, [FromQuery] int? reviewedUserId = null, [FromQuery] int? orderId = null)
    {
        var result = await _reviewService.GetReviewsAsync(page, size, reviewerId, reviewedUserId, orderId);
        return Ok(result);
    }

    [HttpGet("{reviewId}")]
    public async Task<IActionResult> GetReview(int reviewId)
    {
        var result = await _reviewService.GetReviewByIdAsync(reviewId);
        if (result.Code == 404) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var currentUserId = ResolveCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));

        var result = await _reviewService.CreateReviewAsync(request.OrderId, request.Rating, request.Content, currentUserId.Value);
        if (result.Code != 200) return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpPut("{reviewId}")]
    public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewRequest request)
    {
        var currentUserId = ResolveCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));

        var result = await _reviewService.UpdateReviewAsync(reviewId, request.Rating, request.Content, currentUserId.Value);
        if (result.Code != 200) return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var currentUserId = ResolveCurrentUserId();
        if (!currentUserId.HasValue)
            return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));

        var result = await _reviewService.DeleteReviewAsync(reviewId, currentUserId.Value);
        if (result.Code != 200) return StatusCode(result.Code, result);
        return Ok(result);
    }

    private int? ResolveCurrentUserId()
    {
        var token = ReadBearerToken();
        return _authService.TryGetUserIdFromToken(token);
    }

    private string? ReadBearerToken()
    {
        var value = Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : value.Trim();
    }
}