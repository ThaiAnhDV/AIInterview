using AIInterviewPlatform.Application.DTOs.Recommendation;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IRecommendationService
{
    Task<List<RecommendationResponse>> GenerateAndSaveRecommendationsAsync(
        long userId,
        long skillGapAnalysisId,
        List<MissingSkillInput> missingSkills);

    Task<List<RecommendationResponse>> GetMyRecommendationsAsync(long userId);

    Task<List<RecommendationResponse>> GetRecommendationsByAnalysisIdAsync(
        long userId,
        long skillGapAnalysisId);

    Task<RecommendationResponse?> GetRecommendationByIdAsync(
        long userId,
        long recommendationId);
}

public class MissingSkillInput
{
    public long SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? GapDescription { get; set; }
}
