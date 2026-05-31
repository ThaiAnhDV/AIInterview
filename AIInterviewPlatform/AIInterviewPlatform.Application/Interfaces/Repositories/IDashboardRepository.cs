using AIInterviewPlatform.Application.DTOs.Dashboard;
using AIInterviewPlatform.Domain.Enities;

namespace AIInterviewPlatform.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<List<ReadinessScore>> GetReadinessScoresAsync(long userId, int count);
    Task<List<SkillGapAnalysis>> GetSkillGapAnalysesAsync(long userId);
    Task<SkillGapAnalysis?> GetLatestSkillGapAnalysisAsync(long userId);
    Task<List<SkillGap>> GetSkillGapsByAnalysisIdAsync(long analysisId);
    Task<List<ReadinessTimelineDto>> GetReadinessTimelineAsync(long userId, int days);
}
