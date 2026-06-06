namespace AIInterviewPlatform.Application.DTOs.AI;

public class AIConnectionTestResponse
{
    public bool Success { get; set; }
    public bool Connected { get; set; }
    public string Model { get; set; } = "gemini-2.5-flash";
    public long ResponseTimeMs { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorType { get; set; }
    public int? HttpStatus { get; set; }
    public string? GeminiMessage { get; set; }
    public string? ResponseBody { get; set; }
}
