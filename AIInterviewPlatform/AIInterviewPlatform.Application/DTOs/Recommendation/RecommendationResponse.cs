using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Application.DTOs.Recommendation;

public class RecommendationResponse
{
    public long Id { get; set; }
    public long SkillGapAnalysisId { get; set; }
    public long SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string RecommendationTitle { get; set; } = string.Empty;
    public string RecommendationContent { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;
    public string PriorityLevel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
