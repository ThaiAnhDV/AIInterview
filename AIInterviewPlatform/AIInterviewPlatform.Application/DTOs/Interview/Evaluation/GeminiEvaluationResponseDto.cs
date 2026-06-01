namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class GeminiEvaluationResponseDto
{
    public decimal Clarity { get; set; }
    public decimal Structure { get; set; }
    public decimal Relevance { get; set; }
    public decimal Overall { get; set; }
    public string? Feedback { get; set; }
    public string? Improvement { get; set; }
}
