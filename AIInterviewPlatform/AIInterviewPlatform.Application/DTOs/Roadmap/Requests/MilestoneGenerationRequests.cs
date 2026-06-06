namespace AIInterviewPlatform.Application.DTOs.Roadmap.Requests;

public class GenerateMilestonesRequest
{
    public List<string> MissingSkills { get; set; } = [];
    public bool IncludeMockInterview { get; set; } = true;
    public int? MaxMilestones { get; set; }
}

public class GenerateRoadmapFromSkillGapsRequest
{
    public long SkillGapAnalysisId { get; set; }
    public bool IncludeMockInterview { get; set; } = true;
}

public class RoadmapGenerationOptions
{
    public bool IncludeMockInterview { get; set; } = true;
    public bool SortByPriority { get; set; } = true;
    public bool NormalizeSkillNames { get; set; } = true;
    public int? MaxMilestones { get; set; }
    public int ActivitiesPerMilestone { get; set; } = 3;
}
