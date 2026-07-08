using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 聊天模块 Repository 接口（F14）
/// 数据库: chat_session / chat_message 表
///         sp_get_or_create_session / sp_send_message / v_unread_messages / fn_unread_count
/// </summary>
public interface IChatRepository
{
    /// <summary>获取/创建会话 — 调用 sp_get_or_create_session</summary>
    Task<int> GetOrCreateSessionAsync(int goodsId, int buyerId, int sellerId);

    /// <summary>我的会话列表</summary>
    Task<List<ChatSessionDto>> GetSessionsAsync(int userId);

    /// <summary>会话详情</summary>
    Task<ChatSessionDto?> GetSessionByIdAsync(int sessionId);

    /// <summary>会话消息列表</summary>
    Task<List<ChatMessageDto>> GetMessagesAsync(int sessionId, int page = 1, int size = 50);

    /// <summary>发送消息 — 调用 sp_send_message</summary>
    Task<int> SendMessageAsync(int sessionId, int senderId, string content);

    /// <summary>标记会话已读</summary>
    Task<bool> MarkSessionReadAsync(int sessionId, int userId);

    /// <summary>标记单条消息已读</summary>
    Task<bool> MarkMessageReadAsync(int messageId);

    /// <summary>未读消息数 — 调用 fn_unread_count</summary>
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>未读消息列表 — 查 v_unread_messages 视图</summary>
    Task<List<ChatMessageDto>> GetUnreadMessagesAsync(int userId);
}
