namespace CampusTrade.Backend.Models.DTOs;

/// <summary>
/// 会话 DTO — 对应 chat_session 表实体
/// 实体属性: session_id(PK), goods_id(FK→goods), buyer_id(FK→app_user), seller_id(FK→app_user), created_at
/// 约束: UNIQUE(goods_id, buyer_id, seller_id) — 同一买卖双方针对同一商品仅一个会话窗口
/// 关系: User 1:N Session(通过 buyer_id / seller_id), Goods 1:N Session(通过 goods_id)
/// </summary>
public class ChatSessionDto
{
    public int SessionId { get; set; }
    public int GoodsId { get; set; }
    public string? GoodsTitle { get; set; }       // JOIN goods.title 冗余，便于前端展示
    public int BuyerId { get; set; }
    public string? BuyerName { get; set; }         // JOIN app_user.nickname，便于前端展示对方身份
    public int SellerId { get; set; }
    public string? SellerName { get; set; }        // JOIN app_user.nickname
    public DateTime CreateTime { get; set; }
    public int UnreadCount { get; set; }           // 当前用户在该会话中的未读消息数（计算字段）
    public bool IsNew { get; set; }                // 本次是否新建（true=新建, false=已存在）
}

/// <summary>
/// 消息 DTO — 对应 chat_message 表实体
/// 实体属性: message_id(PK), session_id(FK→chat_session), sender_id(FK→app_user), content(CLOB), is_read(0/1), created_at
/// 关系: Session 1:N Message(通过 session_id), User 1:N Message(通过 sender_id)
/// </summary>
public class ChatMessageDto
{
    public int MessageId { get; set; }
    public int SessionId { get; set; }
    public int SenderId { get; set; }
    public string? SenderName { get; set; }        // JOIN app_user.nickname
    public string Content { get; set; } = string.Empty;
    public int ReadStatus { get; set; }            // 0=未读, 1=已读 (映射 is_read)
    public DateTime SendTime { get; set; }         // 映射 created_at
}

/// <summary>创建会话请求 — POST /api/chat/sessions</summary>
public class CreateSessionRequest
{
    public int GoodsId { get; set; }
    public int SellerId { get; set; }
}

/// <summary>发送消息请求 — POST /api/chat/messages</summary>
public class SendMessageRequest
{
    public int SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
}
