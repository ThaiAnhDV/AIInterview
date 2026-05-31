namespace AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

public class MilestoneGenerationResultDto
{
    public List<MilestoneDto> Milestones { get; set; } = [];
    public int TotalMilestones { get; set; }
    public int TotalActivities { get; set; }
    public string EstimatedTotalDuration { get; set; } = string.Empty;
    public List<string> ProcessedSkills { get; set; } = [];
}

public class RoadmapGenerationResultDto
{
    public RoadmapDto Roadmap { get; set; } = new();
    public MilestoneGenerationResultDto MilestonesResult { get; set; } = new();
}
