namespace AIInterviewPlatform.Application.DTOs.Assistant;

public class AssistantChatRequest
{
    public string Message { get; set; } = string.Empty;

    public string? Page { get; set; }
}
