namespace AIInterviewPlatform.Application.DTOs.AI;

public class AIHealthResponse
{
    public bool Gemini { get; set; }
    public string Model { get; set; } = "gemini-2.5-flash";
    public bool SkillExtraction { get; set; }
    public bool JDExtraction { get; set; }
    public bool QuestionGeneration { get; set; }
    public bool Evaluation { get; set; }
    public bool RoadmapGeneration { get; set; }
    public DateTime Timestamp { get; set; }
}
