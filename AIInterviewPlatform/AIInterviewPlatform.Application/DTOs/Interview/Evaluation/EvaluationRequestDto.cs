namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class EvaluationRequestDto
{
    public required string Question { get; init; }
    public required string Answer { get; init; }
    public string? Category { get; init; }
    public string? SkillFocus { get; init; }
}

public class EvaluationResultDto
{
    public bool Success { get; set; } = true;
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public bool IsFallback { get; set; }
    public decimal Clarity { get; set; }
    public decimal Structure { get; set; }
    public decimal Relevance { get; set; }
    public decimal Overall { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string Improvement { get; set; } = string.Empty;
}
