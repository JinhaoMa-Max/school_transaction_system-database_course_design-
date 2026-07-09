using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IAuthService _authService;

    public ChatController(IChatService chatService, IAuthService authService)
    {
        _chatService = chatService;
        _authService = authService;
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var currentUserId = ResolveCurrentUserId();
        var result = await _chatService.GetSessionsAsync(currentUserId);
        if (result.Code != 200)
            return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpGet("sessions/{sessionId:int}")]
    public async Task<IActionResult> GetSession(int sessionId)
    {
        var currentUserId = ResolveCurrentUserId();
        var result = await _chatService.GetSessionByIdAsync(sessionId, currentUserId);
        if (result.Code == 404)
            return NotFound(result);
        if (result.Code == 403)
            return StatusCode(403, result);
        if (result.Code != 200)
            return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var currentUserId = ResolveCurrentUserId();
        var result = await _chatService.CreateSessionAsync(request, currentUserId);
        if (result.Code != 200)
            return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpGet("sessions/{sessionId:int}/messages")]
    public async Task<IActionResult> GetMessages(int sessionId, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var currentUserId = ResolveCurrentUserId();
        var result = await _chatService.GetMessagesAsync(sessionId, page, size, currentUserId);
        if (result.Code == 404)
            return NotFound(result);
        if (result.Code == 403)
            return StatusCode(403, result);
        if (result.Code != 200)
            return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var currentUserId = ResolveCurrentUserId();
        var result = await _chatService.SendMessageAsync(request, currentUserId);
        if (result.Code == 404)
            return NotFound(result);
        if (result.Code == 403)
            return StatusCode(403, result);
        if (result.Code != 200)
            return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpPut("sessions/{sessionId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int sessionId)
    {
        var currentUserId = ResolveCurrentUserId();
        var result = await _chatService.MarkSessionAsReadAsync(sessionId, currentUserId);
        if (result.Code == 404)
            return NotFound(result);
        if (result.Code == 403)
            return StatusCode(403, result);
        if (result.Code != 200)
            return StatusCode(result.Code, result);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUserId = ResolveCurrentUserId();
        var result = await _chatService.GetUnreadCountAsync(currentUserId);
        if (result.Code != 200)
            return StatusCode(result.Code, result);
        return Ok(result);
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
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : value.Trim();
    }
}