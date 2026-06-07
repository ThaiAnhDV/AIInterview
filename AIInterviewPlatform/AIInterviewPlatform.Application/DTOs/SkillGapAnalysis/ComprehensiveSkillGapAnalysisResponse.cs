namespace AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;

public class ComprehensiveSkillGapAnalysisResponse
{
    public bool Success { get; set; } = true;
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public long AnalysisId { get; set; }
    public long ResumeId { get; set; }
    public long JobDescriptionId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public decimal ReadinessScore { get; set; }
    public List<string> ResumeSkills { get; set; } = [];
    public List<string> RequiredSkills { get; set; } = [];
    public List<string> MatchedSkills { get; set; } = [];
    public List<SkillGapItemResponse> MissingSkills { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
