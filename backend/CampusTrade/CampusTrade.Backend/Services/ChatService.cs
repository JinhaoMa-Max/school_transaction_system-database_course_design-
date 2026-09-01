using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IGoodsRepository _goodsRepository;

    public ChatService(IChatRepository chatRepository, IGoodsRepository goodsRepository)
    {
        _chatRepository = chatRepository;
        _goodsRepository = goodsRepository;
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

    public async Task<ApiResponse<ChatSessionDto>> CreateSessionAsync(CreateSessionRequest request, int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<ChatSessionDto>.Fail(401, "未登录");

        var goods = await _goodsRepository.GetByIdAsync(request.GoodsId);
        if (goods == null)
            return ApiResponse<ChatSessionDto>.Fail(404, "商品不存在");
        if (goods.SellerId != request.SellerId)
            return ApiResponse<ChatSessionDto>.Fail(400, "卖家与商品不匹配");
        if (request.SellerId == userId.Value)
            return ApiResponse<ChatSessionDto>.Fail(400, "不能与自己创建会话");

        var sessionId = await _chatRepository.GetOrCreateSessionAsync(
            request.GoodsId,
            userId.Value,
            request.SellerId);

        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
            return ApiResponse<ChatSessionDto>.Fail(500, "会话创建失败");

        return ApiResponse<ChatSessionDto>.Success(session, "会话创建成功");
    }

    public async Task<ApiResponse<ChatMessageListResult>> GetMessagesAsync(int sessionId, int page, int size, int? userId)
    {
        if (!userId.HasValue)
            return ApiResponse<ChatMessageListResult>.Fail(401, "未登录");
        if (page < 1 || size < 1 || size > 100)
            return ApiResponse<ChatMessageListResult>.Fail(400, "分页参数不合法");

        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
            return ApiResponse<ChatMessageListResult>.Fail(404, "会话不存在");

        if (session.BuyerId != userId.Value && session.SellerId != userId.Value)
            return ApiResponse<ChatMessageListResult>.Fail(403, "无权访问该会话");

        var messages = await _chatRepository.GetMessagesAsync(sessionId, page, size);
        return ApiResponse<ChatMessageListResult>.Success(new ChatMessageListResult
        {
            List = messages,
            Total = messages.Count,
            Page = page,
            Size = size
        });
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

        request.Content = request.Content?.Trim() ?? string.Empty;
        if (request.Content.Length == 0)
            return ApiResponse<int>.Fail(400, "消息内容不能为空");
        if (request.Content.Length > 2000)
            return ApiResponse<int>.Fail(400, "消息内容不能超过2000字");

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
