using AIInterviewPlatform.Application.DTOs.Roadmap;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class LearningRoadmapService : ILearningRoadmapService
    {
        private readonly ApplicationDbContext _context;

        public LearningRoadmapService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RoadmapDetailResponse> GenerateRoadmapAsync(
            long userId,
            GenerateRoadmapRequest request)
        {
            var analysis = await _context.SkillGapAnalyses
                .Include(x => x.SkillGaps)
                    .ThenInclude(x => x.Skill)
                .FirstOrDefaultAsync(x =>
                    x.Id == request.SkillGapAnalysisId &&
                    x.UserId == userId);

            if (analysis == null)
            {
                throw new Exception("Skill gap analysis not found.");
            }

            var roadmap = new LearningRoadmap
            {
                UserId = userId,
                SkillGapAnalysisId = request.SkillGapAnalysisId,
                TargetJobId = analysis.JobDescription.TargetJobId,
                RoadmapTitle = "Personal Learning Roadmap",
                RoadmapStatus = RoadmapStatus.ACTIVE,
                CreatedAt = DateTime.Now
            };

            _context.LearningRoadmaps.Add(roadmap);
            await _context.SaveChangesAsync();

            var order = 1;

            foreach (var gap in analysis.SkillGaps)
            {
                var skillName = gap.Skill.SkillName;

                var milestone = new RoadmapMilestone
                {
                    LearningRoadmapId = roadmap.Id,
                    MilestoneTitle = $"Learn {skillName}",
                    MilestoneOrder = order++,
                    IsCompleted = false
                };

                _context.RoadmapMilestones.Add(milestone);
                await _context.SaveChangesAsync();

                _context.LearningActivities.AddRange(
                    new LearningActivity
                    {
                        RoadmapMilestoneId = milestone.Id,
                        SkillId = gap.SkillId,
                        ActivityTitle = $"Read {skillName} fundamentals",
                        ActivityDescription = $"Learn the basic concepts of {skillName}.",
                        ActivityType = ActivityType.READING,
                        IsCompleted = false
                    },
                    new LearningActivity
                    {
                        RoadmapMilestoneId = milestone.Id,
                        SkillId = gap.SkillId,
                        ActivityTitle = $"Practice {skillName}",
                        ActivityDescription = $"Do a small practice task related to {skillName}.",
                        ActivityType = ActivityType.PRACTICE,
                        IsCompleted = false
                    },
                    new LearningActivity
                    {
                        RoadmapMilestoneId = milestone.Id,
                        SkillId = gap.SkillId,
                        ActivityTitle = $"Mock interview for {skillName}",
                        ActivityDescription = $"Practice interview questions related to {skillName}.",
                        ActivityType = ActivityType.MOCK_INTERVIEW,
                        IsCompleted = false
                    }
                );
            }

            await _context.SaveChangesAsync();

            var progress = new RoadmapProgress
            {
                LearningRoadmapId = roadmap.Id,
                CompletionPercentage = 0,
                LastUpdatedAt = DateTime.Now
            };

            _context.RoadmapProgresses.Add(progress);
            await _context.SaveChangesAsync();

            var result = await GetRoadmapByIdAsync(userId, roadmap.Id);

            if (result == null)
            {
                throw new Exception("Roadmap generated but cannot be loaded.");
            }

            return result;
        }

        public async Task<List<RoadmapResponse>> GetMyRoadmapsAsync(
            long userId)
        {
            return await _context.LearningRoadmaps
                .Include(x => x.RoadmapProgress)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new RoadmapResponse
                {
                    Id = x.Id,
                    RoadmapTitle = x.RoadmapTitle,
                    RoadmapStatus = x.RoadmapStatus.ToString(),
                    CompletionPercentage =
                        x.RoadmapProgress != null
                            ? x.RoadmapProgress.CompletionPercentage
                            : 0,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<RoadmapDetailResponse?> GetRoadmapByIdAsync(
            long userId,
            long roadmapId)
        {
            var roadmap = await _context.LearningRoadmaps
                .Include(x => x.RoadmapProgress)
                .Include(x => x.RoadmapMilestones)
                    .ThenInclude(x => x.LearningActivities)
                .FirstOrDefaultAsync(x =>
                    x.Id == roadmapId &&
                    x.UserId == userId);

            if (roadmap == null)
            {
                return null;
            }

            return new RoadmapDetailResponse
            {
                Id = roadmap.Id,
                RoadmapTitle = roadmap.RoadmapTitle,
                RoadmapStatus = roadmap.RoadmapStatus.ToString(),
                CompletionPercentage =
                    roadmap.RoadmapProgress != null
                        ? roadmap.RoadmapProgress.CompletionPercentage
                        : 0,
                CreatedAt = roadmap.CreatedAt,
                Milestones = roadmap.RoadmapMilestones
                    .OrderBy(x => x.MilestoneOrder)
                    .Select(m => new RoadmapMilestoneResponse
                    {
                        Id = m.Id,
                        MilestoneTitle = m.MilestoneTitle,
                        MilestoneOrder = m.MilestoneOrder,
                        IsCompleted = m.IsCompleted,
                        Activities = m.LearningActivities
                            .Select(a => new RoadmapActivityResponse
                            {
                                Id = a.Id,
                                ActivityTitle = a.ActivityTitle,
                                ActivityDescription = a.ActivityDescription,
                                ActivityType = a.ActivityType != null
                                    ? a.ActivityType.ToString()
                                    : null,
                                IsCompleted = a.IsCompleted
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        public async Task<bool> CompleteActivityAsync(
            long userId,
            long activityId)
        {
            var activity = await _context.LearningActivities
                .Include(x => x.RoadmapMilestone)
                    .ThenInclude(x => x.LearningRoadmap)
                .FirstOrDefaultAsync(x =>
                    x.Id == activityId &&
                    x.RoadmapMilestone.LearningRoadmap.UserId == userId);

            if (activity == null)
            {
                return false;
            }

            activity.IsCompleted = true;

            await _context.SaveChangesAsync();

            await UpdateRoadmapProgressAsync(
                activity.RoadmapMilestone.LearningRoadmapId);

            return true;
        }

        private async Task UpdateRoadmapProgressAsync(long roadmapId)
        {
            var roadmap = await _context.LearningRoadmaps
                .Include(x => x.RoadmapMilestones)
                    .ThenInclude(x => x.LearningActivities)
                .Include(x => x.RoadmapProgress)
                .FirstOrDefaultAsync(x => x.Id == roadmapId);

            if (roadmap == null)
            {
                return;
            }

            var activities = roadmap.RoadmapMilestones
                .SelectMany(x => x.LearningActivities)
                .ToList();

            var total = activities.Count;

            var completed = activities.Count(x => x.IsCompleted);

            var percentage = total == 0
                ? 0
                : Math.Round((decimal)completed / total * 100, 2);

            if (roadmap.RoadmapProgress == null)
            {
                roadmap.RoadmapProgress = new RoadmapProgress
                {
                    LearningRoadmapId = roadmap.Id
                };
            }

            roadmap.RoadmapProgress.CompletionPercentage = percentage;
            roadmap.RoadmapProgress.LastUpdatedAt = DateTime.Now;

            foreach (var milestone in roadmap.RoadmapMilestones)
            {
                milestone.IsCompleted =
                    milestone.LearningActivities.Any() &&
                    milestone.LearningActivities.All(x => x.IsCompleted);
            }

            if (percentage >= 100)
            {
                roadmap.RoadmapStatus = RoadmapStatus.COMPLETED;
            }

            roadmap.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}