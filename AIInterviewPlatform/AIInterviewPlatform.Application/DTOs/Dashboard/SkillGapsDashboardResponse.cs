namespace AIInterviewPlatform.Application.DTOs.Dashboard;

public class SkillGapDto
{
    public long SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillType { get; set; } = string.Empty;
    public string GapLevel { get; set; } = string.Empty;
    public string? GapDescription { get; set; }
    public DateTime AnalysisDate { get; set; }
}

public class SkillGapsDashboardResponse
{
    public long LatestAnalysisId { get; set; }
    public DateTime AnalysisDate { get; set; }
    public int TotalMissingSkills { get; set; }
    public List<SkillGapDto> MissingSkills { get; set; } = [];
}
