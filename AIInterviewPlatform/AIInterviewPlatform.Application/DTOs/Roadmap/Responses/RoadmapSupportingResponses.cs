namespace AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

public class SkillGapForRoadmapDto
{
    public long SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillType { get; set; } = string.Empty;
    public string GapLevel { get; set; } = string.Empty;
    public string? GapDescription { get; set; }
}

public class RoadmapSkillAnalysisDto
{
    public long SkillGapAnalysisId { get; set; }
    public List<SkillGapForRoadmapDto> MissingSkills { get; set; } = [];
    public List<SkillGapForRoadmapDto> MatchedSkills { get; set; } = [];
    public int TotalMissingSkills { get; set; }
    public decimal ReadinessScore { get; set; }
}

public class RoadmapTemplateDto
{
    public string TemplateName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecommendedMilestones { get; set; }
    public int RecommendedActivitiesPerMilestone { get; set; }
    public List<string> TargetSkillTypes { get; set; } = [];
}

public class RoadmapStatisticsDto
{
    public int TotalRoadmaps { get; set; }
    public int ActiveRoadmaps { get; set; }
    public int CompletedRoadmaps { get; set; }
    public decimal AverageCompletionPercentage { get; set; }
    public int TotalActivitiesCompleted { get; set; }
    public int TotalMilestonesCompleted { get; set; }
    public string MostPracticedSkill { get; set; } = string.Empty;
    public int TotalLearningDays { get; set; }
}
