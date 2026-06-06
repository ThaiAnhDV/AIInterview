namespace AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

public class ActivityDescriptionRequest
{
    public string SkillName { get; set; } = string.Empty;
    public string? DifficultyLevel { get; set; }
}

public class ActivityDescriptionResponse
{
    public string ActivityTitle { get; set; } = string.Empty;
    public string ActivityDescription { get; set; } = string.Empty;
}
