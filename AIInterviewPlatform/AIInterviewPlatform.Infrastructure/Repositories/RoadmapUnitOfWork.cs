using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace AIInterviewPlatform.Infrastructure.Repositories;

public class RoadmapUnitOfWork : IRoadmapUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRoadmapRepository? _roadmaps;
    private IMilestoneRepository? _milestones;
    private IActivityRepository? _activities;
    private IProgressRepository? _progress;

    public RoadmapUnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRoadmapRepository Roadmaps =>
        _roadmaps ??= new RoadmapRepository(_context);

    public IMilestoneRepository Milestones =>
        _milestones ??= new MilestoneRepository(_context);

    public IActivityRepository Activities =>
        _activities ??= new ActivityRepository(_context);

    public IProgressRepository Progress =>
        _progress ??= new ProgressRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<(bool Success, string? ErrorMessage, long RoadmapId)> SaveRoadmapWithDetailsAsync(
        LearningRoadmap roadmap,
        List<RoadmapMilestone> milestones,
        List<LearningActivity> activities,
        RoadmapProgress progress)
    {
        try
        {
            await BeginTransactionAsync();

            await Roadmaps.AddAsync(roadmap);
            await SaveChangesAsync();

            if (milestones.Count > 0)
            {
                foreach (var milestone in milestones)
                {
                    milestone.LearningRoadmapId = roadmap.Id;
                }
                await Milestones.AddRangeAsync(milestones);
                await SaveChangesAsync();
            }

            if (activities.Count > 0)
            {
                var milestoneDict = milestones.ToDictionary(m => m.MilestoneOrder);
                
                foreach (var activity in activities)
                {
                    if (milestoneDict.TryGetValue(GetMilestoneOrder(activity), out var milestone))
                    {
                        activity.RoadmapMilestoneId = milestone.Id;
                    }
                }
                await Activities.AddRangeAsync(activities);
                await SaveChangesAsync();
            }

            progress.LearningRoadmapId = roadmap.Id;
            await Progress.AddAsync(progress);
            await SaveChangesAsync();

            await CommitTransactionAsync();

            return (true, null, roadmap.Id);
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync();
            
            return (false, ex.Message, 0);
        }
    }

    public async Task<(bool Success, string? ErrorMessage, long MilestoneId, bool MilestoneCompleted, decimal MilestoneProgress, decimal RoadmapProgress)> CompleteActivityAsync(long activityId)
    {
        try
        {
            await BeginTransactionAsync();

            var activity = await Activities.GetByIdWithDetailsAsync(activityId);
            if (activity == null)
            {
                return (false, "Activity not found", 0, false, 0, 0);
            }

            activity.IsCompleted = true;
            await Activities.UpdateAsync(activity);
            await SaveChangesAsync();

            var milestone = await Milestones.GetByIdWithActivitiesAsync(activity.RoadmapMilestoneId);
            long milestoneId = milestone?.Id ?? 0;
            bool milestoneCompleted = false;
            decimal milestoneProgress = 0;

            if (milestone != null)
            {
                var allCompleted = milestone.LearningActivities.All(a => a.IsCompleted);
                milestone.IsCompleted = allCompleted;
                await Milestones.UpdateAsync(milestone);
                await SaveChangesAsync();
                milestoneCompleted = allCompleted;
                milestoneProgress = CalculateProgress(milestone);

                var roadmap = await Roadmaps.GetByIdWithDetailsAsync(milestone.LearningRoadmapId);
                if (roadmap != null)
                {
                    await UpdateRoadmapProgressAsync(roadmap);
                    await SaveChangesAsync();
                }
            }

            await CommitTransactionAsync();

            return (true, null, milestoneId, milestoneCompleted, milestoneProgress, 0);
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync();
            
            return (false, ex.Message, 0, false, 0, 0);
        }
    }

    private async Task UpdateRoadmapProgressAsync(LearningRoadmap roadmap)
    {
        var allActivities = roadmap.RoadmapMilestones
            .SelectMany(m => m.LearningActivities)
            .ToList();

        var completedCount = allActivities.Count(a => a.IsCompleted);
        var percentage = allActivities.Count == 0
            ? 0
            : Math.Round((decimal)completedCount / allActivities.Count * 100, 2);

        var progress = await Progress.GetByRoadmapIdAsync(roadmap.Id);
        if (progress != null)
        {
            progress.CompletionPercentage = percentage;
            progress.LastUpdatedAt = DateTime.UtcNow;
            await Progress.UpdateAsync(progress);
        }

        roadmap.UpdatedAt = DateTime.UtcNow;
        if (percentage >= 100)
        {
            roadmap.RoadmapStatus = Domain.Enum.RoadmapStatus.COMPLETED;
        }
        await Roadmaps.UpdateAsync(roadmap);
    }

    private static int GetMilestoneOrder(LearningActivity activity)
    {
        return 1;
    }

    private static decimal CalculateProgress(RoadmapMilestone? milestone)
    {
        if (milestone == null || !milestone.LearningActivities.Any())
            return 0;

        var completed = milestone.LearningActivities.Count(a => a.IsCompleted);
        return Math.Round((decimal)completed / milestone.LearningActivities.Count * 100, 2);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

public class RoadmapPersistenceResult
{
    public bool Success { get; set; }
    public long RoadmapId { get; set; }
    public int MilestonesCount { get; set; }
    public int ActivitiesCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ActivityCompletionResult
{
    public bool Success { get; set; }
    public long ActivityId { get; set; }
    public long MilestoneId { get; set; }
    public bool MilestoneCompleted { get; set; }
    public decimal MilestoneProgress { get; set; }
    public decimal RoadmapProgress { get; set; }
    public string? ErrorMessage { get; set; }
}
