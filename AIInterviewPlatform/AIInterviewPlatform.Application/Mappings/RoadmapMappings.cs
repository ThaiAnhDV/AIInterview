using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Domain.Enities;

namespace AIInterviewPlatform.Application.Mappings;

public static class RoadmapMappings
{
    public static RoadmapDto ToDto(this LearningRoadmap roadmap)
    {
        return new RoadmapDto
        {
            Id = roadmap.Id,
            RoadmapTitle = roadmap.RoadmapTitle,
            RoadmapStatus = roadmap.RoadmapStatus.ToString(),
            TargetJobId = roadmap.TargetJobId,
            SkillGapAnalysisId = roadmap.SkillGapAnalysisId,
            Progress = roadmap.RoadmapProgress?.ToDto(),
            Milestones = roadmap.RoadmapMilestones
                .OrderBy(m => m.MilestoneOrder)
                .Select(m => m.ToDto())
                .ToList(),
            CreatedAt = roadmap.CreatedAt,
            UpdatedAt = roadmap.UpdatedAt
        };
    }

    public static RoadmapSummaryDto ToSummaryDto(this LearningRoadmap roadmap)
    {
        var milestones = roadmap.RoadmapMilestones.ToList();
        var activities = milestones.SelectMany(m => m.LearningActivities).ToList();

        return new RoadmapSummaryDto
        {
            Id = roadmap.Id,
            RoadmapTitle = roadmap.RoadmapTitle,
            RoadmapStatus = roadmap.RoadmapStatus.ToString(),
            TotalMilestones = milestones.Count,
            CompletedMilestones = milestones.Count(m => m.IsCompleted),
            TotalActivities = activities.Count,
            CompletedActivities = activities.Count(a => a.IsCompleted),
            CompletionPercentage = CalculateCompletionPercentage(activities),
            CreatedAt = roadmap.CreatedAt
        };
    }

    public static MilestoneDto ToDto(this RoadmapMilestone milestone)
    {
        return new MilestoneDto
        {
            Id = milestone.Id,
            LearningRoadmapId = milestone.LearningRoadmapId,
            MilestoneTitle = milestone.MilestoneTitle,
            MilestoneOrder = milestone.MilestoneOrder,
            IsCompleted = milestone.IsCompleted,
            CompletionPercentage = CalculateMilestoneCompletion(milestone),
            Activities = milestone.LearningActivities
                .OrderBy(a => a.Id)
                .Select(a => a.ToDto())
                .ToList(),
            CompletedAt = milestone.IsCompleted ? DateTime.Now : null
        };
    }

    public static MilestoneSummaryDto ToSummaryDto(this RoadmapMilestone milestone)
    {
        var activities = milestone.LearningActivities.ToList();

        return new MilestoneSummaryDto
        {
            Id = milestone.Id,
            MilestoneTitle = milestone.MilestoneTitle,
            MilestoneOrder = milestone.MilestoneOrder,
            IsCompleted = milestone.IsCompleted,
            TotalActivities = activities.Count,
            CompletedActivities = activities.Count(a => a.IsCompleted),
            CompletionPercentage = CalculateMilestoneCompletion(milestone)
        };
    }

    public static ActivityDto ToDto(this LearningActivity activity)
    {
        return new ActivityDto
        {
            Id = activity.Id,
            RoadmapMilestoneId = activity.RoadmapMilestoneId,
            SkillId = activity.SkillId,
            SkillName = activity.Skill?.SkillName,
            ActivityTitle = activity.ActivityTitle,
            ActivityDescription = activity.ActivityDescription,
            ActivityType = activity.ActivityType?.ToString() ?? string.Empty,
            IsCompleted = activity.IsCompleted,
            CompletedAt = activity.IsCompleted ? DateTime.Now : null,
            Metadata = new ActivityMetadataDto
            {
                Tags = ExtractTags(activity.ActivityTitle)
            }
        };
    }

    public static ActivitySummaryDto ToSummaryDto(this LearningActivity activity)
    {
        return new ActivitySummaryDto
        {
            Id = activity.Id,
            ActivityTitle = activity.ActivityTitle,
            ActivityType = activity.ActivityType?.ToString() ?? string.Empty,
            IsCompleted = activity.IsCompleted
        };
    }

    public static RoadmapProgressDto ToDto(this RoadmapProgress progress)
    {
        return new RoadmapProgressDto
        {
            Id = progress.Id,
            OverallProgress = progress.CompletionPercentage,
            TotalMilestones = progress.LearningRoadmap?.RoadmapMilestones.Count ?? 0,
            CompletedMilestones = progress.LearningRoadmap?.RoadmapMilestones
                .Count(m => m.IsCompleted) ?? 0,
            TotalActivities = progress.LearningRoadmap?.RoadmapMilestones
                .SelectMany(m => m.LearningActivities).Count() ?? 0,
            CompletedActivities = progress.LearningRoadmap?.RoadmapMilestones
                .SelectMany(m => m.LearningActivities).Count(a => a.IsCompleted) ?? 0,
            LastActivityAt = progress.LastUpdatedAt
        };
    }

    public static SkillGapForRoadmapDto ToRoadmapDto(this SkillGap skillGap)
    {
        return new SkillGapForRoadmapDto
        {
            SkillId = skillGap.SkillId,
            SkillName = skillGap.Skill.SkillName,
            SkillType = skillGap.Skill.SkillType ?? "Technology",
            GapLevel = skillGap.GapLevel?.ToString() ?? "MEDIUM",
            GapDescription = skillGap.GapDescription
        };
    }

    public static List<SkillGapForRoadmapDto> ToRoadmapDto(
        this IEnumerable<SkillGap> skillGaps)
    {
        return skillGaps.Select(g => g.ToRoadmapDto()).ToList();
    }

    public static RoadmapMilestone ToEntity(this MilestoneDto dto, long roadmapId)
    {
        return new RoadmapMilestone
        {
            LearningRoadmapId = roadmapId,
            MilestoneTitle = dto.MilestoneTitle,
            MilestoneOrder = dto.MilestoneOrder,
            IsCompleted = dto.IsCompleted
        };
    }

    public static LearningActivity ToEntity(this ActivityDto dto, long milestoneId)
    {
        return new LearningActivity
        {
            RoadmapMilestoneId = milestoneId,
            SkillId = dto.SkillId,
            ActivityTitle = dto.ActivityTitle,
            ActivityDescription = dto.ActivityDescription,
            ActivityType = ParseActivityType(dto.ActivityType),
            IsCompleted = dto.IsCompleted
        };
    }

    private static decimal CalculateCompletionPercentage(List<LearningActivity> activities)
    {
        if (activities.Count == 0) return 0;
        var completed = activities.Count(a => a.IsCompleted);
        return Math.Round((decimal)completed / activities.Count * 100, 2);
    }

    private static decimal CalculateMilestoneCompletion(RoadmapMilestone milestone)
    {
        var activities = milestone.LearningActivities.ToList();
        if (activities.Count == 0) return 0;
        var completed = activities.Count(a => a.IsCompleted);
        return Math.Round((decimal)completed / activities.Count * 100, 2);
    }

    private static List<string> ExtractTags(string title)
    {
        var words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Take(3).ToList();
    }

    private static Domain.Enum.ActivityType ParseActivityType(string type)
    {
        return type?.ToUpperInvariant() switch
        {
            "READING" => Domain.Enum.ActivityType.READING,
            "PRACTICE" => Domain.Enum.ActivityType.PRACTICE,
            "MOCK_INTERVIEW" => Domain.Enum.ActivityType.MOCK_INTERVIEW,
            "QUIZ" => Domain.Enum.ActivityType.QUIZ,
            _ => Domain.Enum.ActivityType.OTHER
        };
    }
}
