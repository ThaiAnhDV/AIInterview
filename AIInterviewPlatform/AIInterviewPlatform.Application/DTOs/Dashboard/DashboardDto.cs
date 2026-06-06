namespace AIInterviewPlatform.Application.DTOs.Dashboard;

public class MissingSkillDto
{
    public long SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? SkillType { get; set; }
    public string GapLevel { get; set; } = string.Empty;
    public string? GapDescription { get; set; }
    public int Priority { get; set; }
}

public class RecentFeedbackDto
{
    public long FeedbackId { get; set; }
    public long InterviewAnswerId { get; set; }
    public string FeedbackType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DashboardDto
{
    public ReadinessSummaryDto Readiness { get; set; } = new();
    public SkillGapSummaryDto SkillGaps { get; set; } = new();
    public InterviewSummaryDto Interviews { get; set; } = new();
    public RoadmapProgressSummaryDto RoadmapProgress { get; set; } = new();
    public List<RecentFeedbackDto> RecentFeedbacks { get; set; } = [];
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ReadinessSummaryDto
{
    public decimal? CurrentScore { get; set; }
    public decimal? PreviousScore { get; set; }
    public decimal ImprovementPercentage { get; set; }
    public string Trend { get; set; } = "STABLE";
    public DateTime? CalculatedAt { get; set; }
}

public class SkillGapSummaryDto
{
    public int TotalMissingSkills { get; set; }
    public int HighPriorityCount { get; set; }
    public int MediumPriorityCount { get; set; }
    public int LowPriorityCount { get; set; }
    public List<MissingSkillDto> MissingSkills { get; set; } = [];
    public DateTime? LastAnalyzedAt { get; set; }
}

public class InterviewSummaryDto
{
    public int TotalInterviews { get; set; }
    public int CompletedInterviews { get; set; }
    public int PendingInterviews { get; set; }
    public decimal AverageScore { get; set; }
    public decimal? HighestScore { get; set; }
    public decimal? LowestScore { get; set; }
}

public class RoadmapProgressSummaryDto
{
    public int TotalRoadmaps { get; set; }
    public decimal OverallProgressPercentage { get; set; }
    public int CompletedMilestones { get; set; }
    public int TotalMilestones { get; set; }
    public string? ActiveRoadmapTitle { get; set; }
    public decimal? ActiveRoadmapProgress { get; set; }
}
