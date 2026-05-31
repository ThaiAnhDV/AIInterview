using AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface ISkillGapAnalysisOrchestratorService
{
    Task<ComprehensiveSkillGapAnalysisResponse> AnalyzeSkillGapAsync(
        long userId,
        ComprehensiveSkillGapAnalysisRequest request);
}
