namespace CampusTrade.Backend.Models.DTOs;

public class AiChatRequest
{
    public List<AiChatMessage>? Messages { get; set; }
}

public class AiChatMessage
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
