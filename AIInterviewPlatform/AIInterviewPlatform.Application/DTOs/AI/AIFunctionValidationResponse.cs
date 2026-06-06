namespace AIInterviewPlatform.Application.DTOs.AI;

public class AIFunctionValidationResponse
{
    public bool Success { get; set; }
    public bool SkillExtraction { get; set; }
    public bool JDExtraction { get; set; }
    public bool QuestionGeneration { get; set; }
    public bool Evaluation { get; set; }
    public bool RoadmapGeneration { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
