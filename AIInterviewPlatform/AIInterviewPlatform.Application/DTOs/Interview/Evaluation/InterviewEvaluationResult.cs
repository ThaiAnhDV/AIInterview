namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class InterviewEvaluationResult
{
    public required EvaluationScores Scores { get; init; }
    public required EvaluationFeedback Feedback { get; init; }
    public required List<ImprovementSuggestion> Improvements { get; init; }
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;
}

public class EvaluationScores
{
    public required ScoreDetail Clarity { get; init; }
    public required ScoreDetail Structure { get; init; }
    public required ScoreDetail Relevance { get; init; }
    public required ScoreDetail Overall { get; init; }
}

public class ScoreDetail
{
    public decimal Value { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
}

public class EvaluationFeedback
{
    public required string Summary { get; init; }
    public required List<string> Strengths { get; init; }
    public required List<string> Weaknesses { get; init; }
    public required List<string> DetailedFeedback { get; init; }
}

public class ImprovementSuggestion
{
    public required string Area { get; init; }
    public required string Suggestion { get; init; }
    public required Priority Priority { get; init; }
    public string? ExampleTip { get; init; }
}

public enum Priority
{
    High,
    Medium,
    Low
}
