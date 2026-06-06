using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

namespace AIInterviewPlatform.Application.DTOs.Roadmap.Requests;

public class GenerateRoadmapFromMissingSkillsRequest
{
    public List<SkillGapForRoadmapDto> MissingSkills { get; set; } = [];
    public long? TargetJobId { get; set; }
    public int MilestonesPerSkill { get; set; } = 2;
    public int ActivitiesPerMilestone { get; set; } = 3;
}

public class GenerateRoadmapFromAnalysisRequest
{
    public long SkillGapAnalysisId { get; set; }
    public int MilestonesPerSkill { get; set; } = 2;
    public int ActivitiesPerMilestone { get; set; } = 3;
}

public class UpdateRoadmapRequest
{
    public string? RoadmapTitle { get; set; }
    public string? RoadmapStatus { get; set; }
}

public class UpdateMilestoneRequest
{
    public string? MilestoneTitle { get; set; }
    public bool? IsCompleted { get; set; }
}

public class UpdateActivityRequest
{
    public string? ActivityTitle { get; set; }
    public string? ActivityDescription { get; set; }
    public string? ActivityType { get; set; }
    public bool? IsCompleted { get; set; }
}

public class CompleteActivityRequest
{
    public long ActivityId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CompleteMilestoneRequest
{
    public long MilestoneId { get; set; }
    public bool IsCompleted { get; set; }
}
