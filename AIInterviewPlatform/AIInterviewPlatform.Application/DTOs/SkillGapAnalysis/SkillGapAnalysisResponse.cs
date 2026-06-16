using AIInterviewPlatform.Application.DTOs.Recommendation;

namespace AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;

public class SkillGapAnalysisResponse
{
    public bool Success { get; set; } = true;
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public long Id { get; set; }
    public long ResumeId { get; set; }
    public long JobDescriptionId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public decimal ReadinessScore { get; set; }
    public List<string> MatchedSkills { get; set; } = new();
    public List<SkillGapItemResponse> MissingSkills { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    // FR-012: Strengths and Weaknesses from StrengthWeaknessReports
    public List<StrengthWeaknessReportResponse> Strengths { get; set; } = new();
    public List<StrengthWeaknessReportResponse> Weaknesses { get; set; } = new();

    // Recommendation Engine MVP: Recommendations for missing skills
    public List<RecommendationResponse> Recommendations { get; set; } = new();
}
