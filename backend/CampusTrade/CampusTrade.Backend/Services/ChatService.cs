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

    public async Task<ChatSessionDto> GetOrCreateSessionAsync(int goodsId, int buyerId, int sellerId)
    {
        if (goodsId <= 0) throw new ArgumentException("goodsId 不合法");
        if (buyerId <= 0) throw new ArgumentException("buyerId 不合法");
        if (sellerId <= 0) throw new ArgumentException("sellerId 不合法");
        if (buyerId == sellerId) throw new InvalidOperationException("不能与自己发起私聊");

        // 校验商品存在
        var goods = await _goodsRepository.GetByIdAsync(goodsId);
        if (goods == null) throw new ArgumentException("商品不存在");

        // 利用 UNIQUE(goods_id,buyer_id,seller_id) 约束，先查是否已存在
        var existingId = await _chatRepository.FindSessionIdAsync(goodsId, buyerId, sellerId);
        bool isNew = existingId == null;

        var sessionId = await _chatRepository.GetOrCreateSessionAsync(goodsId, buyerId, sellerId);
        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session != null)
        {
            session.IsNew = isNew;
        }
        return session!;
    }

    public Task<List<ChatSessionDto>> GetSessionsAsync(int userId)
    {
        return _chatRepository.GetSessionsAsync(userId);
    }

    public Task<ChatSessionDto?> GetSessionByIdAsync(int sessionId)
    {
        return _chatRepository.GetSessionByIdAsync(sessionId);
    }

    public Task<List<ChatMessageDto>> GetMessagesAsync(int sessionId, int page = 1, int size = 50)
    {
        if (page < 1) page = 1;
        if (size < 1) size = 50;
        return _chatRepository.GetMessagesAsync(sessionId, page, size);
    }

    public async Task<ChatMessageDto> SendMessageAsync(int sessionId, int senderId, string content)
    {
        if (sessionId <= 0) throw new ArgumentException("sessionId 不合法");
        if (senderId <= 0) throw new ArgumentException("senderId 不合法");
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("消息内容不能为空");

        // 校验会话存在且当前用户是会话参与者（buyer 或 seller）
        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session == null) throw new ArgumentException("会话不存在");
        if (session.BuyerId != senderId && session.SellerId != senderId)
            throw new UnauthorizedAccessException("你不在该会话中");

        // 调用存储过程发送消息，获得 messageId
        var msgId = await _chatRepository.SendMessageAsync(sessionId, senderId, content);

        // 回查完整的消息 DTO（含 senderName）
        var msg = await _chatRepository.GetMessageByIdAsync(msgId);
        return msg!;
    }

    public Task<bool> MarkSessionReadAsync(int sessionId, int userId)
    {
        return _chatRepository.MarkSessionReadAsync(sessionId, userId);
    }

    public Task<int> GetUnreadCountAsync(int userId)
    {
        return _chatRepository.GetUnreadCountAsync(userId);
    }
}
