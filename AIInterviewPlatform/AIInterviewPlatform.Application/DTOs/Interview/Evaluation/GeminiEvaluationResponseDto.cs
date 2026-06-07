namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class GeminiEvaluationResponseDto
{
    public decimal ClarityScore { get; set; }
    public decimal TechnicalAccuracyScore { get; set; }
    public decimal CompletenessScore { get; set; }
    public decimal OverallScore { get; set; }
    public List<string>? Strengths { get; set; }
    public List<string>? Weaknesses { get; set; }
    public string? Feedback { get; set; }
}
