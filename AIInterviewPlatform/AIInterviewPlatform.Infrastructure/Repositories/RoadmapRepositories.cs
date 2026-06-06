using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Repositories;

public class RoadmapRepository : GenericRepository<LearningRoadmap>, IRoadmapRepository
{
    public RoadmapRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<LearningRoadmap?> GetByIdWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(r => r.RoadmapProgress)
            .Include(r => r.RoadmapMilestones)
                .ThenInclude(m => m.LearningActivities)
                    .ThenInclude(a => a.Skill)
            .Include(r => r.TargetJob)
            .Include(r => r.SkillGapAnalysis)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<LearningRoadmap>> GetByUserIdAsync(long userId)
    {
        return await _dbSet
            .Include(r => r.RoadmapProgress)
            .Include(r => r.RoadmapMilestones)
                .ThenInclude(m => m.LearningActivities)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<LearningRoadmap> AddAsync(LearningRoadmap roadmap)
    {
        await _dbSet.AddAsync(roadmap);
        return roadmap;
    }

    public async Task UpdateAsync(LearningRoadmap roadmap)
    {
        _dbSet.Update(roadmap);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(long id)
    {
        var roadmap = await _dbSet.FindAsync(id);
        if (roadmap != null)
        {
            _dbSet.Remove(roadmap);
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _dbSet.AnyAsync(r => r.Id == id);
    }
}

public class MilestoneRepository : GenericRepository<RoadmapMilestone>, IMilestoneRepository
{
    public MilestoneRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RoadmapMilestone?> GetByIdWithActivitiesAsync(long id)
    {
        return await _dbSet
            .Include(m => m.LearningActivities)
                .ThenInclude(a => a.Skill)
            .Include(m => m.LearningRoadmap)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<RoadmapMilestone>> GetByRoadmapIdAsync(long roadmapId)
    {
        return await _dbSet
            .Include(m => m.LearningActivities)
            .Where(m => m.LearningRoadmapId == roadmapId)
            .OrderBy(m => m.MilestoneOrder)
            .ToListAsync();
    }

    public async Task<RoadmapMilestone> AddAsync(RoadmapMilestone milestone)
    {
        await _dbSet.AddAsync(milestone);
        return milestone;
    }

    public async Task AddRangeAsync(IEnumerable<RoadmapMilestone> milestones)
    {
        await _dbSet.AddRangeAsync(milestones);
    }

    public async Task UpdateAsync(RoadmapMilestone milestone)
    {
        _dbSet.Update(milestone);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(long id)
    {
        var milestone = await _dbSet.FindAsync(id);
        if (milestone != null)
        {
            _dbSet.Remove(milestone);
        }
    }
}

public class ActivityRepository : GenericRepository<LearningActivity>, IActivityRepository
{
    public ActivityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<LearningActivity?> GetByIdWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(a => a.Skill)
            .Include(a => a.RoadmapMilestone)
                .ThenInclude(m => m.LearningRoadmap)
            .Include(a => a.PracticeHistories)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<LearningActivity>> GetByMilestoneIdAsync(long milestoneId)
    {
        return await _dbSet
            .Include(a => a.Skill)
            .Where(a => a.RoadmapMilestoneId == milestoneId)
            .ToListAsync();
    }

    public async Task<LearningActivity> AddAsync(LearningActivity activity)
    {
        await _dbSet.AddAsync(activity);
        return activity;
    }

    public async Task AddRangeAsync(IEnumerable<LearningActivity> activities)
    {
        await _dbSet.AddRangeAsync(activities);
    }

    public async Task UpdateAsync(LearningActivity activity)
    {
        _dbSet.Update(activity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(long id)
    {
        var activity = await _dbSet.FindAsync(id);
        if (activity != null)
        {
            _dbSet.Remove(activity);
        }
    }
}

public class ProgressRepository : GenericRepository<RoadmapProgress>, IProgressRepository
{
    public ProgressRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RoadmapProgress?> GetByRoadmapIdAsync(long roadmapId)
    {
        return await _dbSet
            .Include(p => p.LearningRoadmap)
            .FirstOrDefaultAsync(p => p.LearningRoadmapId == roadmapId);
    }

    public async Task<RoadmapProgress> AddAsync(RoadmapProgress progress)
    {
        await _dbSet.AddAsync(progress);
        return progress;
    }

    public async Task UpdateAsync(RoadmapProgress progress)
    {
        _dbSet.Update(progress);
        await Task.CompletedTask;
    }
}
