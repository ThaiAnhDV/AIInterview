namespace AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;

public class ComprehensiveSkillGapAnalysisResponse
{
    public long AnalysisId { get; set; }
    public long ResumeId { get; set; }
    public long JobDescriptionId { get; set; }
    public decimal ReadinessScore { get; set; }
    public List<string> MatchedSkills { get; set; } = [];
    public List<SkillGapItemResponse> MissingSkills { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
