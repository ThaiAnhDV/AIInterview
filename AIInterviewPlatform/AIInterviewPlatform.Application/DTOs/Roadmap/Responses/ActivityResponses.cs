namespace AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

public class ActivityDto
{
    public long Id { get; set; }
    public long RoadmapMilestoneId { get; set; }
    public long? SkillId { get; set; }
    public string? SkillName { get; set; }
    public string ActivityTitle { get; set; } = string.Empty;
    public string? ActivityDescription { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ActivityMetadataDto? Metadata { get; set; }
}

public class ActivitySummaryDto
{
    public long Id { get; set; }
    public string ActivityTitle { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class ActivityMetadataDto
{
    public string? EstimatedDuration { get; set; }
    public string? DifficultyLevel { get; set; }
    public string? ResourceUrl { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? Prerequisites { get; set; }
}

public class ActivityWithMilestoneDto
{
    public ActivityDto Activity { get; set; } = new();
    public MilestoneSummaryDto Milestone { get; set; } = new();
}

public class ActivityCompletionDto
{
    public long ActivityId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CompletedAt { get; set; }
    public ActivityCompletionResultDto Result { get; set; } = new();
}

public class ActivityCompletionResultDto
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public long MilestoneId { get; set; }
    public bool IsMilestoneCompleted { get; set; }
    public decimal MilestoneProgress { get; set; }
    public decimal RoadmapProgress { get; set; }
    public int RemainingActivities { get; set; }
}
