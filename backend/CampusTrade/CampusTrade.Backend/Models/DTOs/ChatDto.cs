namespace CampusTrade.Backend.Models.DTOs;

/// <summary>会话 DTO — 对应 chat_session 表</summary>
public class ChatSessionDto
{
    public int SessionId { get; set; }
    public int GoodsId { get; set; }
    public string? GoodsTitle { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public DateTime CreateTime { get; set; }
    public int UnreadCount { get; set; }    // 当前用户的未读数
}

/// <summary>消息 DTO — 对应 chat_message 表</summary>
public class ChatMessageDto
{
    public int MessageId { get; set; }
    public int SessionId { get; set; }
    public int SenderId { get; set; }
    public string? SenderName { get; set; }
    public string Content { get; set; } = string.Empty;     // CLOB
    public int ReadStatus { get; set; }                     // 0=未读 1=已读 → 前端 isRead
    public DateTime SendTime { get; set; }                  // created_at
}

/// <summary>发送消息请求</summary>
public class SendMessageRequest { public int SessionId { get; set; } public string Content { get; set; } = string.Empty; }
