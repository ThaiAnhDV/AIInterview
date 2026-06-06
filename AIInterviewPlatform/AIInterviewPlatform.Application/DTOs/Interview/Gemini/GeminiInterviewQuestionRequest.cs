namespace AIInterviewPlatform.Application.DTOs.Interview.Gemini;

public class GeminiInterviewQuestionRequest
{
    public required TargetJobInfo TargetJob { get; init; }
    public required List<string> RequiredSkills { get; init; }
    public List<string> MissingSkills { get; init; } = [];
    public QuestionGenerationConfig Config { get; init; } = new();
}

public class TargetJobInfo
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Company { get; init; }
    public string? Location { get; init; }
}

public class QuestionGenerationConfig
{
    public int TechnicalQuestions { get; init; } = 5;
    public int BehavioralQuestions { get; init; } = 3;
    public int CommunicationQuestions { get; init; } = 2;
}

public class GeminiInterviewQuestionResponse
{
    public List<GeminiQuestion> Questions { get; set; } = [];
}

public class GeminiQuestion
{
    public string Question { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? SkillFocus { get; set; }
}
