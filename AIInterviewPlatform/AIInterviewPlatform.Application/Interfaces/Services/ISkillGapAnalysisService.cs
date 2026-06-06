using AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface ISkillGapAnalysisService
    {
        Task<SkillGapAnalysisResponse> AnalyzeAsync(
            long userId,
            CreateSkillGapAnalysisRequest request);

        Task<List<SkillGapAnalysisResponse>> GetMyAnalysesAsync(
            long userId);

        Task<SkillGapAnalysisResponse?> GetByIdAsync(
            long userId,
            long analysisId);
    }
}