using AIInterviewPlatform.Application.DTOs.Interview.Enums;

namespace AIInterviewPlatform.Application.DTOs.Interview.Models;

public class QuestionDto
{
    public long? QuestionId { get; init; }
    public Guid QuestionGuid { get; init; } = Guid.NewGuid();
    public required string QuestionContent { get; init; }
    public required string QuestionType { get; init; }
    public required string Difficulty { get; init; }
    public required string Category { get; init; }
    public required string SkillArea { get; init; }
    public string? ExpectedAnswerFramework { get; init; }
    public string? SampleAnswer { get; init; }
    public int SuggestedTimeMinutes { get; init; } = 5;
    public List<FollowUpQuestionDto> FollowUpQuestions { get; init; } = [];
    public List<string> EvaluationCriteria { get; init; } = [];
    public Dictionary<string, string> Metadata { get; init; } = [];
}

public class FollowUpQuestionDto
{
    public Guid FollowUpId { get; init; } = Guid.NewGuid();
    public required string QuestionContent { get; init; }
    public string? ProbeType { get; init; }
    public string? Purpose { get; init; }
}

public class TechnicalQuestionDto : QuestionDto
{
    public required List<string> TechnicalConcepts { get; init; }
    public string? CodeSnippet { get; init; }
    public string? ProblemScenario { get; init; }
    public List<string> RelatedTechnologies { get; init; } = [];
    public string? SystemDesignConsiderations { get; init; }
}

public class BehavioralQuestionDto : QuestionDto
{
    public required string Competency { get; init; }
    public required string Situation { get; init; }
    public string? CompanyCultureAlignment { get; init; }
    public List<string> LeadershipIndicators { get; init; } = [];
    public List<string> SoftSkillIndicators { get; init; } = [];
}

public class CommunicationQuestionDto : QuestionDto
{
    public required string CommunicationScenario { get; init; }
    public required string StakeholderType { get; init; }
    public string? ConflictScenario { get; init; }
    public List<string> CommunicationChannels { get; init; } = [];
    public string? PresentationContext { get; init; }
}
