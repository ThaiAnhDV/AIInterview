using AIInterviewPlatform.Application.DTOs.Interview.Enums;
using AIInterviewPlatform.Application.DTOs.Interview.Models;

namespace AIInterviewPlatform.Application.DTOs.Interview.Responses;

public class InterviewQuestionGenerationResult
{
    public Guid GenerationId { get; init; } = Guid.NewGuid();
    public long? SessionId { get; init; }
    public required TargetJobSummary TargetJob { get; init; }
    public required GenerationSummary Summary { get; init; }
    public required List<QuestionDto> Questions { get; init; }
    public GenerationMetadata Metadata { get; init; } = new();
    public GenerationStatusEnum Status { get; init; } = GenerationStatusEnum.Pending;
    public string? ErrorMessage { get; init; }
    public bool IsFallback { get; init; }
}

public class TargetJobSummary
{
    public long JobId { get; init; }
    public required string Title { get; init; }
    public string? Company { get; init; }
}

public class GenerationSummary
{
    public int TotalQuestionsGenerated { get; init; }
    public QuestionCountByType ByType { get; init; } = new();
    public QuestionCountByDifficulty ByDifficulty { get; init; } = new();
    public List<string> SkillsCovered { get; init; } = [];
    public List<string> SkillsToFocus { get; init; } = [];
    public TimeSpan EstimatedDuration { get; init; }
}

public class QuestionCountByType
{
    public int Technical { get; init; }
    public int Behavioral { get; init; }
    public int Communication { get; init; }
    public int Total => Technical + Behavioral + Communication;
}

public class QuestionCountByDifficulty
{
    public int Beginner { get; init; }
    public int Intermediate { get; init; }
    public int Advanced { get; init; }
    public int Total => Beginner + Intermediate + Advanced;
}

public class GenerationMetadata
{
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public string? AiModelUsed { get; init; }
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public string? GenerationVersion { get; init; }
    public Dictionary<string, object> CustomMetadata { get; init; } = [];
}

public class QuestionGenerationFailure
{
    public required string Reason { get; init; }
    public string? SuggestedRetryAction { get; init; }
    public Exception? OriginalException { get; init; }
}
