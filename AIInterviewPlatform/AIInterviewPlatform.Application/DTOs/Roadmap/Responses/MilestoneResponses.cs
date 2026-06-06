namespace AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

public class MilestoneDto
{
    public long Id { get; set; }
    public long LearningRoadmapId { get; set; }
    public string MilestoneTitle { get; set; } = string.Empty;
    public int MilestoneOrder { get; set; }
    public bool IsCompleted { get; set; }
    public decimal CompletionPercentage { get; set; }
    public List<ActivityDto> Activities { get; set; } = [];
    public DateTime? CompletedAt { get; set; }
    public MilestoneMetadataDto? Metadata { get; set; }
}

public class MilestoneSummaryDto
{
    public long Id { get; set; }
    public string MilestoneTitle { get; set; } = string.Empty;
    public int MilestoneOrder { get; set; }
    public bool IsCompleted { get; set; }
    public int TotalActivities { get; set; }
    public int CompletedActivities { get; set; }
    public decimal CompletionPercentage { get; set; }
}

public class MilestoneMetadataDto
{
    public string? TargetSkill { get; set; }
    public string? SkillType { get; set; }
    public string? GapLevel { get; set; }
    public string? EstimatedDuration { get; set; }
    public string? DifficultyLevel { get; set; }
    public List<string> LearningObjectives { get; set; } = [];
}

public class MilestoneWithRoadmapDto
{
    public MilestoneDto Milestone { get; set; } = new();
    public RoadmapSummaryDto Roadmap { get; set; } = new();
}
