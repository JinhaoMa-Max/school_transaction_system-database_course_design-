using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IChatService
{
    Task<ApiResponse<List<ChatSessionDto>>> GetSessionsAsync(int? userId);
    Task<ApiResponse<ChatSessionDto>> GetSessionByIdAsync(int sessionId, int? userId);
    Task<ApiResponse<int>> CreateSessionAsync(CreateSessionRequest request, int? userId);
    Task<ApiResponse<List<ChatMessageDto>>> GetMessagesAsync(int sessionId, int page, int size, int? userId);
    Task<ApiResponse<int>> SendMessageAsync(SendMessageRequest request, int? userId);
    Task<ApiResponse<bool>> MarkSessionAsReadAsync(int sessionId, int? userId);
    Task<ApiResponse<int>> GetUnreadCountAsync(int? userId);
}