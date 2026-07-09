using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IChatService
{
    /// <summary>获取或创建会话（用于从商品详情发起私聊）</summary>
    Task<ChatSessionDto> GetOrCreateSessionAsync(int goodsId, int buyerId, int sellerId);

    /// <summary>获取我的会话列表（含未读数和商品标题）</summary>
    Task<List<ChatSessionDto>> GetSessionsAsync(int userId);

    /// <summary>获取会话详情</summary>
    Task<ChatSessionDto?> GetSessionByIdAsync(int sessionId);

    /// <summary>获取会话消息列表（分页）</summary>
    Task<List<ChatMessageDto>> GetMessagesAsync(int sessionId, int page = 1, int size = 50);

    /// <summary>发送消息</summary>
    Task<ChatMessageDto> SendMessageAsync(int sessionId, int senderId, string content);

    /// <summary>标记会话已读</summary>
    Task<bool> MarkSessionReadAsync(int sessionId, int userId);

    /// <summary>获取未读消息数</summary>
    Task<int> GetUnreadCountAsync(int userId);
}
