using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;

    public ChatService(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<ApiResponse<List<ChatSessionDto>>> GetSessionsAsync(int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<List<ChatSessionDto>>.Fail(401, "未登录");

        var sessions = await _chatRepository.GetSessionsAsync(userId.Value);
        return ApiResponse<List<ChatSessionDto>>.Success(sessions);
    }

    public async Task<ApiResponse<ChatSessionDto>> GetSessionByIdAsync(int sessionId, int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<ChatSessionDto>.Fail(401, "未登录");

        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
            return ApiResponse<ChatSessionDto>.Fail(404, "会话不存在");

        if (session.BuyerId != userId.Value && session.SellerId != userId.Value)
            return ApiResponse<ChatSessionDto>.Fail(403, "无权访问该会话");

        return ApiResponse<ChatSessionDto>.Success(session);
    }

    public async Task<ApiResponse<int>> CreateSessionAsync(CreateSessionRequest request, int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<int>.Fail(401, "未登录");

        var sessionId = await _chatRepository.GetOrCreateSessionAsync(
            request.GoodsId,
            userId.Value,
            request.SellerId);

        return ApiResponse<int>.Success(sessionId, "会话创建成功");
    }

    public async Task<ApiResponse<List<ChatMessageDto>>> GetMessagesAsync(int sessionId, int page, int size, int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<List<ChatMessageDto>>.Fail(401, "未登录");

        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
            return ApiResponse<List<ChatMessageDto>>.Fail(404, "会话不存在");

        if (session.BuyerId != userId.Value && session.SellerId != userId.Value)
            return ApiResponse<List<ChatMessageDto>>.Fail(403, "无权访问该会话");

        var messages = await _chatRepository.GetMessagesAsync(sessionId, page, size);
        return ApiResponse<List<ChatMessageDto>>.Success(messages);
    }

    public async Task<ApiResponse<int>> SendMessageAsync(SendMessageRequest request, int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<int>.Fail(401, "未登录");

        var session = await _chatRepository.GetSessionByIdAsync(request.SessionId);
        if (session == null)
            return ApiResponse<int>.Fail(404, "会话不存在");

        if (session.BuyerId != userId.Value && session.SellerId != userId.Value)
            return ApiResponse<int>.Fail(403, "无权发送消息");

        var messageId = await _chatRepository.SendMessageAsync(request.SessionId, userId.Value, request.Content);
        return ApiResponse<int>.Success(messageId, "消息发送成功");
    }

    public async Task<ApiResponse<bool>> MarkSessionAsReadAsync(int sessionId, int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<bool>.Fail(401, "未登录");

        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
            return ApiResponse<bool>.Fail(404, "会话不存在");

        if (session.BuyerId != userId.Value && session.SellerId != userId.Value)
            return ApiResponse<bool>.Fail(403, "无权操作");

        await _chatRepository.MarkSessionReadAsync(sessionId, userId.Value);
        return ApiResponse<bool>.Success(true, "已标记为已读");
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<int>.Fail(401, "未登录");

        var count = await _chatRepository.GetUnreadCountAsync(userId.Value);
        return ApiResponse<int>.Success(count);
    }
}