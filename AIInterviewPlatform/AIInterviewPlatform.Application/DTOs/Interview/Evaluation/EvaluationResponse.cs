using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class EvaluationResponse
{
    public long AnswerId { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
    public bool AiUsed { get; set; }
    public string GeneratedBy { get; set; } = "GEMINI";
    public string? ErrorMessage { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public decimal Clarity { get; set; }
    public decimal TechnicalAccuracy { get; set; }
    public decimal Completeness { get; set; }
    public decimal Overall { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public string Feedback { get; set; } = string.Empty;
}
