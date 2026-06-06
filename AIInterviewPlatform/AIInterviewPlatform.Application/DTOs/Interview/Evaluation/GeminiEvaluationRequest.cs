namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class GeminiEvaluationRequest
{
    public required string Question { get; init; }
    public required string Answer { get; init; }
    public string? QuestionCategory { get; init; }
    public string? QuestionSkillFocus { get; init; }
    public EvaluationPromptConfig Config { get; init; } = new();
}

public class EvaluationPromptConfig
{
    public bool RequestImprovementTips { get; init; } = true;
    public bool RequestDetailedFeedback { get; init; } = true;
    public int MaxSuggestions { get; init; } = 3;
}

public class GeminiEvaluationResponse
{
    public GeminiScores Scores { get; set; } = new();
    public GeminiFeedback Feedback { get; set; } = new();
    public List<GeminiImprovement> Improvements { get; set; } = new();
}

public class GeminiScores
{
    public decimal Clarity { get; set; }
    public decimal Structure { get; set; }
    public decimal Relevance { get; set; }
    public decimal Overall { get; set; }
    public string? ClarityReasoning { get; set; }
    public string? StructureReasoning { get; set; }
    public string? RelevanceReasoning { get; set; }
    public string? OverallReasoning { get; set; }
}

public class GeminiFeedback
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> DetailedFeedback { get; set; } = new();
}

public class GeminiImprovement
{
    public string Area { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public string? ExampleTip { get; set; }
}
