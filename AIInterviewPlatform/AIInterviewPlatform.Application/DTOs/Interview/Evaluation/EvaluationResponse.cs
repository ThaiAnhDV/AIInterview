using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class EvaluationResponse
{
    public long AnswerId { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
    public decimal Clarity { get; set; }
    public decimal Structure { get; set; }
    public decimal Relevance { get; set; }
    public decimal Overall { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string Improvement { get; set; } = string.Empty;
}
