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
    Task<List<Feedback>> GetRecentFeedbacksAsync(long userId, int count);
    Task<int> GetInterviewCountAsync(long userId);
    Task<int> GetCompletedInterviewCountAsync(long userId);
    Task<decimal?> GetAverageInterviewScoreAsync(long userId);
    Task<decimal?> GetHighestInterviewScoreAsync(long userId);
    Task<decimal?> GetLowestInterviewScoreAsync(long userId);
    Task<List<LearningRoadmap>> GetRoadmapsAsync(long userId);
    Task<RoadmapProgress?> GetRoadmapProgressAsync(long roadmapId);
}
