using AIInterviewPlatform.Domain.Enities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Application.Interfaces.Repositories;

public interface IRoadmapUnitOfWork
{
    IRoadmapRepository Roadmaps { get; }
    IMilestoneRepository Milestones { get; }
    IActivityRepository Activities { get; }
    IProgressRepository Progress { get; }
    IRoadmapRecommendationRepository RoadmapRecommendationsRepo { get; }

    Task<(bool Success, string? ErrorMessage, long RoadmapId)> SaveRoadmapWithDetailsAsync(
        LearningRoadmap roadmap,
        List<RoadmapMilestone> milestones,
        List<LearningActivity> activities,
        RoadmapProgress progress,
        List<RoadmapRecommendation>? roadmapRecommendations = null);

    Task<(bool Success, string? ErrorMessage, long MilestoneId, bool MilestoneCompleted, decimal MilestoneProgress, decimal RoadmapProgress)> CompleteActivityAsync(long activityId);

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> SaveChangesAsync();
    void Dispose();
}
