using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Domain.Enities;

namespace AIInterviewPlatform.Application.Interfaces.Repositories;

public interface IRoadmapRepository
{
    Task<LearningRoadmap?> GetByIdAsync(long id);
    Task<LearningRoadmap?> GetByIdWithDetailsAsync(long id);
    Task<List<LearningRoadmap>> GetByUserIdAsync(long userId);
    Task<LearningRoadmap?> GetActiveByAnalysisIdAsync(long userId, long analysisId);
    Task<LearningRoadmap> AddAsync(LearningRoadmap roadmap);
    Task UpdateAsync(LearningRoadmap roadmap);
    Task DeleteAsync(long id);
    Task<bool> ExistsAsync(long id);
}

public interface IMilestoneRepository
{
    Task<RoadmapMilestone?> GetByIdAsync(long id);
    Task<RoadmapMilestone?> GetByIdWithActivitiesAsync(long id);
    Task<List<RoadmapMilestone>> GetByRoadmapIdAsync(long roadmapId);
    Task<RoadmapMilestone> AddAsync(RoadmapMilestone milestone);
    Task AddRangeAsync(IEnumerable<RoadmapMilestone> milestones);
    Task UpdateAsync(RoadmapMilestone milestone);
    Task DeleteAsync(long id);
}

public interface IActivityRepository
{
    Task<LearningActivity?> GetByIdAsync(long id);
    Task<LearningActivity?> GetByIdWithDetailsAsync(long id);
    Task<List<LearningActivity>> GetByMilestoneIdAsync(long milestoneId);
    Task<LearningActivity> AddAsync(LearningActivity activity);
    Task AddRangeAsync(IEnumerable<LearningActivity> activities);
    Task UpdateAsync(LearningActivity activity);
    Task DeleteAsync(long id);
}

public interface IProgressRepository
{
    Task<RoadmapProgress?> GetByRoadmapIdAsync(long roadmapId);
    Task<RoadmapProgress> AddAsync(RoadmapProgress progress);
    Task UpdateAsync(RoadmapProgress progress);
}

public interface IRoadmapRecommendationRepository
{
    Task<List<RoadmapRecommendation>> GetByRoadmapIdAsync(long roadmapId);
    Task<List<RoadmapRecommendation>> GetByRecommendationIdAsync(long recommendationId);
    Task AddAsync(RoadmapRecommendation roadmapRecommendation);
    Task AddRangeAsync(IEnumerable<RoadmapRecommendation> roadmapRecommendations);
}
