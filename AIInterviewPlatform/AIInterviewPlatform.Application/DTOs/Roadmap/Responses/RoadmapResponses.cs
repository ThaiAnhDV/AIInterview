namespace AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

public class RoadmapDto
{
    public bool Success { get; set; } = true;
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public bool IsAiFallback { get; set; }
    public long Id { get; set; }
    public string RoadmapTitle { get; set; } = string.Empty;
    public string RoadmapStatus { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public long? TargetJobId { get; set; }
    public string? TargetJobTitle { get; set; }
    public long? SkillGapAnalysisId { get; set; }
    public RoadmapProgressDto? Progress { get; set; }
    public List<MilestoneDto> Milestones { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RoadmapSummaryDto
{
    public long Id { get; set; }
    public string RoadmapTitle { get; set; } = string.Empty;
    public string RoadmapStatus { get; set; } = string.Empty;
    public int TotalMilestones { get; set; }
    public int CompletedMilestones { get; set; }
    public int TotalActivities { get; set; }
    public int CompletedActivities { get; set; }
    public decimal CompletionPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoadmapListDto
{
    public List<RoadmapSummaryDto> Roadmaps { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class RoadmapProgressDto
{
    public long Id { get; set; }
    public decimal OverallProgress { get; set; }
    public int TotalMilestones { get; set; }
    public int CompletedMilestones { get; set; }
    public int TotalActivities { get; set; }
    public int CompletedActivities { get; set; }
    public DateTime LastActivityAt { get; set; }
}

public class GeneratedRoadmapDto
{
    public RoadmapDto Roadmap { get; set; } = new();
    public List<string> TargetSkills { get; set; } = [];
    public int EstimatedDaysToComplete { get; set; }
    public string Difficulty { get; set; } = string.Empty;
}
