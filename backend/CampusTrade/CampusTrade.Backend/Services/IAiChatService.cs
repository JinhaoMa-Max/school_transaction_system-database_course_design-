using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IAiChatService
{
    /// <summary>
    /// Streams an AI chat completion to the client. On success the response body is
    /// the upstream SSE stream; on failure a standard ApiResponse envelope is written.
    /// </summary>
    Task ChatAsync(AiChatRequest? request, HttpResponse response, CancellationToken cancellationToken);
}
