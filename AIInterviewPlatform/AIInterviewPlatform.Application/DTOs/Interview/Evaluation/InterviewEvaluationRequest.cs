namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class InterviewEvaluationRequest
{
    public required QuestionContext Question { get; init; }
    public required AnswerContext Answer { get; init; }
    public EvaluationConfig Config { get; init; } = new();
}

public class QuestionContext
{
    public required string Content { get; init; }
    public string? Category { get; init; }
    public string? SkillFocus { get; init; }
    public string? ExpectedKeywords { get; init; }
}

public class AnswerContext
{
    public required string Content { get; init; }
    public TimeSpan? TimeTaken { get; init; }
}

public class EvaluationConfig
{
    public bool IncludeImprovement { get; init; } = true;
    public bool IncludeDetailedFeedback { get; init; } = true;
    public int MaxFeedbackItems { get; init; } = 5;
}
