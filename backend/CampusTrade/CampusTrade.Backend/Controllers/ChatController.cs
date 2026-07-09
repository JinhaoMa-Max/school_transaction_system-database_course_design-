using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IAuthService _authService;

    public ChatController(IChatService chatService, IAuthService authService)
    {
        _chatService = chatService;
        _authService = authService;
    }

    /// <summary>GET /api/chat/sessions — 获取我的会话列表</summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        try
        {
            var userId = ResolveCurrentUserId();
            if (userId == null) return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));
            var list = await _chatService.GetSessionsAsync(userId.Value);
            return Ok(ApiResponse<List<ChatSessionDto>>.Success(list));
        }
        catch (Exception ex) { return ToErrorResult(ex); }
    }

    /// <summary>GET /api/chat/sessions/{sessionId} — 获取会话详情</summary>
    [HttpGet("sessions/{sessionId:int}")]
    public async Task<IActionResult> GetSession(int sessionId)
    {
        try
        {
            var dto = await _chatService.GetSessionByIdAsync(sessionId);
            if (dto == null) return NotFound(ApiResponse<object>.Fail(404, "会话不存在"));
            return Ok(ApiResponse<ChatSessionDto>.Success(dto));
        }
        catch (Exception ex) { return ToErrorResult(ex); }
    }

    /// <summary>
    /// POST /api/chat/sessions — 创建/获取会话（从商品详情发起私聊）
    /// 利用 UNIQUE(goods_id,buyer_id,seller_id) 约束保证同一买家-卖家-商品仅一个会话窗口
    /// </summary>
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            var userId = ResolveCurrentUserId();
            if (userId == null) return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));
            var dto = await _chatService.GetOrCreateSessionAsync(request.GoodsId, userId.Value, request.SellerId);
            var msg = dto.IsNew ? "会话已创建" : "会话已存在，返回已有会话";
            return Ok(ApiResponse<ChatSessionDto>.Success(dto, msg));
        }
        catch (Exception ex) { return ToErrorResult(ex); }
    }

    /// <summary>GET /api/chat/sessions/{sessionId}/messages — 获取会话消息</summary>
    [HttpGet("sessions/{sessionId:int}/messages")]
    public async Task<IActionResult> GetMessages(int sessionId, [FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        try
        {
            var list = await _chatService.GetMessagesAsync(sessionId, page, size);
            return Ok(ApiResponse<object>.Success(new { list, total = list.Count, page, size }));
        }
        catch (Exception ex) { return ToErrorResult(ex); }
    }

    /// <summary>POST /api/chat/messages — 发送消息</summary>
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = ResolveCurrentUserId();
            if (userId == null) return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));
            var dto = await _chatService.SendMessageAsync(request.SessionId, userId.Value, request.Content);
            return Ok(ApiResponse<ChatMessageDto>.Success(dto, "发送成功"));
        }
        catch (Exception ex) { return ToErrorResult(ex); }
    }

    /// <summary>PUT /api/chat/sessions/{sessionId}/read — 标记会话已读</summary>
    [HttpPut("sessions/{sessionId:int}/read")]
    public async Task<IActionResult> MarkRead(int sessionId)
    {
        try
        {
            var userId = ResolveCurrentUserId();
            if (userId == null) return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));
            await _chatService.MarkSessionReadAsync(sessionId, userId.Value);
            return Ok(ApiResponse<object>.Success(new { success = true }, "已标记已读"));
        }
        catch (Exception ex) { return ToErrorResult(ex); }
    }

    /// <summary>GET /api/chat/unread-count — 获取未读消息数</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = ResolveCurrentUserId();
            if (userId == null) return Unauthorized(ApiResponse<object>.Fail(401, "未登录"));
            var count = await _chatService.GetUnreadCountAsync(userId.Value);
            return Ok(ApiResponse<object>.Success(new { count }));
        }
        catch (Exception ex) { return ToErrorResult(ex); }
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
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : value.Trim();
    }

    private IActionResult ToErrorResult(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException uae => Unauthorized(ApiResponse<object>.Fail(401, uae.Message)),
            ArgumentException ae => BadRequest(ApiResponse<object>.Fail(400, ae.Message)),
            InvalidOperationException ioe => BadRequest(ApiResponse<object>.Fail(400, ioe.Message)),
            _ => StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"))
        };
    }
}
