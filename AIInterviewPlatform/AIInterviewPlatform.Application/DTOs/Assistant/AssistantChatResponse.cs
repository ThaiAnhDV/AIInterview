namespace AIInterviewPlatform.Application.DTOs.Assistant;

public class AssistantChatResponse
{
    public string Reply { get; set; } = string.Empty;

    public bool IsFallback { get; set; }

    public string Model { get; set; } = "gemini-2.0-flash";
}
