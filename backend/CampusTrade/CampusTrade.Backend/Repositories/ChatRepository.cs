using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 聊天数据访问层（F14）
/// 数据库: chat_session / chat_message 表
///         sp_get_or_create_session / sp_send_message / fn_unread_count / v_unread_messages
/// </summary>
public class ChatRepository : IChatRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public ChatRepository(IDbConnectionFactory connectionFactory) { _connectionFactory = connectionFactory; }

    /// <summary>查询已存在的会话ID（利用 UNIQUE(goods_id,buyer_id,seller_id) 约束查重）</summary>
    public async Task<int?> FindSessionIdAsync(int goodsId, int buyerId, int sellerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT session_id FROM chat_session
            WHERE goods_id = :G AND buyer_id = :B AND seller_id = :S
            """;
        return await connection.QueryFirstOrDefaultAsync<int?>(sql, new { G = goodsId, B = buyerId, S = sellerId });
    }

    /// <summary>获取/创建会话 — sp_get_or_create_session(goodsId, buyerId, sellerId → sessionId OUT)</summary>
    public async Task<int> GetOrCreateSessionAsync(int goodsId, int buyerId, int sellerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        const string sql = """
            BEGIN sp_get_or_create_session(p_goods_id=>:g, p_buyer_id=>:b, p_seller_id=>:s, p_session_id=>:outId); END;
            """;
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("g", OracleDbType.Int32) { Value = goodsId });
        cmd.Parameters.Add(new OracleParameter("b", OracleDbType.Int32) { Value = buyerId });
        cmd.Parameters.Add(new OracleParameter("s", OracleDbType.Int32) { Value = sellerId });
        var outP = new OracleParameter("outId", OracleDbType.Int32) { Direction = System.Data.ParameterDirection.Output };
        cmd.Parameters.Add(outP);
        await cmd.ExecuteNonQueryAsync();
        return Convert.ToInt32(outP.Value?.ToString());
    }

    /// <summary>我的会话列表（含买卖家昵称）</summary>
    public async Task<List<ChatSessionDto>> GetSessionsAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT cs.session_id AS SessionId, cs.goods_id AS GoodsId, g.title AS GoodsTitle,
                   cs.buyer_id AS BuyerId, bu.nickname AS BuyerName,
                   cs.seller_id AS SellerId, su.nickname AS SellerName,
                   cs.created_at AS CreateTime,
                   (SELECT COUNT(*) FROM chat_message cm WHERE cm.session_id = cs.session_id
                    AND cm.is_read = 0 AND cm.sender_id != :UserId) AS UnreadCount
            FROM chat_session cs
            JOIN goods g ON cs.goods_id = g.goods_id
            LEFT JOIN app_user bu ON cs.buyer_id = bu.user_id
            LEFT JOIN app_user su ON cs.seller_id = su.user_id
            WHERE cs.buyer_id = :UserId OR cs.seller_id = :UserId
            ORDER BY cs.created_at DESC
            """;
        var items = await connection.QueryAsync<ChatSessionDto>(sql, new { UserId = userId });
        return items.ToList();
    }

    public async Task<ChatSessionDto?> GetSessionByIdAsync(int sessionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT cs.session_id AS SessionId, cs.goods_id AS GoodsId, g.title AS GoodsTitle,
                   cs.buyer_id AS BuyerId, bu.nickname AS BuyerName,
                   cs.seller_id AS SellerId, su.nickname AS SellerName,
                   cs.created_at AS CreateTime, 0 AS UnreadCount
            FROM chat_session cs
            JOIN goods g ON cs.goods_id = g.goods_id
            LEFT JOIN app_user bu ON cs.buyer_id = bu.user_id
            LEFT JOIN app_user su ON cs.seller_id = su.user_id
            WHERE cs.session_id = :Id
            """;
        return await connection.QueryFirstOrDefaultAsync<ChatSessionDto>(sql, new { Id = sessionId });
    }

    /// <summary>消息列表（按时间升序）</summary>
    public async Task<List<ChatMessageDto>> GetMessagesAsync(int sessionId, int page = 1, int size = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        var off = (page - 1) * size;
        var sql = $"""
            SELECT cm.message_id AS MessageId, cm.session_id AS SessionId, cm.sender_id AS SenderId,
                   u.nickname AS SenderName, cm.content AS Content, cm.is_read AS ReadStatus,
                   cm.created_at AS SendTime
            FROM chat_message cm LEFT JOIN app_user u ON cm.sender_id = u.user_id
            WHERE cm.session_id = :Sid
            ORDER BY cm.created_at ASC
            OFFSET {off} ROWS FETCH NEXT {size} ROWS ONLY
            """;
        var items = await connection.QueryAsync<ChatMessageDto>(sql, new { Sid = sessionId });
        return items.ToList();
    }

    /// <summary>根据ID获取单条消息（含发送者昵称）</summary>
    public async Task<ChatMessageDto?> GetMessageByIdAsync(int messageId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT cm.message_id AS MessageId, cm.session_id AS SessionId, cm.sender_id AS SenderId,
                   u.nickname AS SenderName, cm.content AS Content, cm.is_read AS ReadStatus,
                   cm.created_at AS SendTime
            FROM chat_message cm LEFT JOIN app_user u ON cm.sender_id = u.user_id
            WHERE cm.message_id = :Mid
            """;
        return await connection.QueryFirstOrDefaultAsync<ChatMessageDto>(sql, new { Mid = messageId });
    }

    /// <summary>发送消息 — sp_send_message(sessionId, senderId, content → messageId OUT)</summary>
    public async Task<int> SendMessageAsync(int sessionId, int senderId, string content)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        const string sql = """
            BEGIN sp_send_message(p_session_id=>:s, p_sender_id=>:u, p_content=>:c, p_message_id=>:outId); END;
            """;
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("s", OracleDbType.Int32) { Value = sessionId });
        cmd.Parameters.Add(new OracleParameter("u", OracleDbType.Int32) { Value = senderId });
        cmd.Parameters.Add(new OracleParameter("c", OracleDbType.Clob) { Value = content });
        var outP = new OracleParameter("outId", OracleDbType.Int32) { Direction = System.Data.ParameterDirection.Output };
        cmd.Parameters.Add(outP);
        await cmd.ExecuteNonQueryAsync();
        return Convert.ToInt32(outP.Value?.ToString());
    }

    /// <summary>标记会话已读 — 对方发的消息全部标为已读</summary>
    public async Task<bool> MarkSessionReadAsync(int sessionId, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "UPDATE chat_message SET is_read=1 WHERE session_id=:s AND sender_id != :u AND is_read=0";
        return await connection.ExecuteAsync(sql, new { s = sessionId, u = userId }) > 0;
    }

    public async Task<bool> MarkMessageReadAsync(int messageId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync("UPDATE chat_message SET is_read=1 WHERE message_id=:m", new { m = messageId }) > 0;
    }

    /// <summary>未读消息数 — fn_unread_count</summary>
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT fn_unread_count(:u) FROM DUAL", new { u = userId });
    }

    /// <summary>未读消息列表 — v_unread_messages 视图</summary>
    public async Task<List<ChatMessageDto>> GetUnreadMessagesAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT message_id AS MessageId, session_id AS SessionId, sender_id AS SenderId,
                   sender_name AS SenderName, content AS Content, is_read AS ReadStatus,
                   created_at AS SendTime
            FROM v_unread_messages WHERE receiver_id = :UserId ORDER BY created_at DESC
            """;
        var items = await connection.QueryAsync<ChatMessageDto>(sql, new { UserId = userId });
        return items.ToList();
    }
}
