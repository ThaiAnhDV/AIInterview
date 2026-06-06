namespace AIInterviewPlatform.Application.DTOs.Interview.Requests;

public class InterviewQuestionGenerationRequest
{
    public required TargetJobDto TargetJob { get; init; }
    public required List<string> RequiredSkills { get; init; }
    public List<string> MissingSkills { get; init; } = [];
    public QuestionGenerationOptions Options { get; init; } = new();
}

public class TargetJobDto
{
    public long JobId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? Company { get; init; }
    public string? Location { get; init; }
    public string? EmploymentType { get; init; }
    public int? ExperienceLevel { get; init; }
}

public class QuestionGenerationOptions
{
    public int TotalQuestions { get; init; } = 10;
    public QuestionTypeDistribution TypeDistribution { get; init; } = new();
    public DifficultyDistribution DifficultyConfig { get; init; } = new();
    public List<string> FocusAreas { get; init; } = [];
    public string? CustomInstructions { get; init; }
    public bool IncludeFollowUpQuestions { get; init; } = true;
    public bool IncludeScenarioBasedQuestions { get; init; } = true;
}

public class QuestionTypeDistribution
{
    public int Technical { get; init; } = 5;
    public int Behavioral { get; init; } = 3;
    public int Communication { get; init; } = 2;

    public int Total => Technical + Behavioral + Communication;
}

public class DifficultyDistribution
{
    public int Beginner { get; init; } = 2;
    public int Intermediate { get; init; } = 5;
    public int Advanced { get; init; } = 3;

    public int Total => Beginner + Intermediate + Advanced;
}
