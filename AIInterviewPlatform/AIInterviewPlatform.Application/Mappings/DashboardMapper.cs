using AIInterviewPlatform.Application.DTOs.Dashboard;
using AIInterviewPlatform.Domain.Enities;

namespace AIInterviewPlatform.Application.Mappings;

public static class DashboardMapper
{
    public static ReadinessSummaryDto ToReadinessSummary(this ReadinessScore? latest, ReadinessScore? previous)
    {
        var improvement = CalculateImprovement(latest, previous);

        return new ReadinessSummaryDto
        {
            CurrentScore = latest?.Score,
            PreviousScore = previous?.Score,
            ImprovementPercentage = improvement,
            Trend = DetermineTrend(latest, previous),
            CalculatedAt = latest?.CalculatedAt
        };
    }

    public static SkillGapSummaryDto ToSkillGapSummary(this SkillGapAnalysis? analysis, List<SkillGap> skillGaps)
    {
        if (analysis == null)
        {
            return new SkillGapSummaryDto();
        }

        return new SkillGapSummaryDto
        {
            TotalMissingSkills = skillGaps.Count,
            HighPriorityCount = skillGaps.Count(x => x.GapLevel == Domain.Enum.GapLevel.HIGH || x.GapLevel == Domain.Enum.GapLevel.CRITICAL),
            MediumPriorityCount = skillGaps.Count(x => x.GapLevel == Domain.Enum.GapLevel.MEDIUM),
            LowPriorityCount = skillGaps.Count(x => x.GapLevel == Domain.Enum.GapLevel.LOW),
            MissingSkills = skillGaps.Select(x => x.ToMissingSkillDto()).ToList(),
            LastAnalyzedAt = analysis.CreatedAt
        };
    }

    public static MissingSkillDto ToMissingSkillDto(this SkillGap skillGap)
    {
        return new MissingSkillDto
        {
            SkillId = skillGap.SkillId,
            SkillName = skillGap.Skill?.SkillName ?? string.Empty,
            SkillType = skillGap.Skill?.SkillType,
            GapLevel = skillGap.GapLevel?.ToString() ?? "UNKNOWN",
            GapDescription = skillGap.GapDescription,
            Priority = skillGap.GapLevel.GetPriority()
        };
    }

    public static InterviewSummaryDto ToInterviewSummary(int total, int completed, decimal? avgScore)
    {
        return new InterviewSummaryDto
        {
            TotalInterviews = total,
            CompletedInterviews = completed,
            PendingInterviews = total - completed,
            AverageScore = avgScore ?? 0
        };
    }

    public static RoadmapProgressSummaryDto ToRoadmapProgressSummary(this IEnumerable<LearningRoadmap> roadmaps)
    {
        var roadmapList = roadmaps.ToList();

        var summary = new RoadmapProgressSummaryDto
        {
            TotalRoadmaps = roadmapList.Count
        };

        if (roadmapList.Count == 0)
        {
            return summary;
        }

        var activeRoadmap = roadmapList
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefault(r => r.RoadmapStatus == Domain.Enum.RoadmapStatus.ACTIVE);

        if (activeRoadmap != null)
        {
            summary.ActiveRoadmapTitle = activeRoadmap.RoadmapTitle;
            summary.ActiveRoadmapProgress = activeRoadmap.RoadmapProgress?.CompletionPercentage ?? 0;
        }

        var allMilestones = roadmapList.SelectMany(r => r.RoadmapMilestones).ToList();
        summary.TotalMilestones = allMilestones.Count;
        summary.CompletedMilestones = allMilestones.Count(m => m.IsCompleted);

        var progressValues = roadmapList
            .Where(r => r.RoadmapProgress != null)
            .Select(r => r.RoadmapProgress!.CompletionPercentage)
            .ToList();

        summary.OverallProgressPercentage = progressValues.Count > 0
            ? Math.Round(progressValues.Average(), 2)
            : 0;

        return summary;
    }

    public static RecentFeedbackDto ToRecentFeedbackDto(this Feedback feedback)
    {
        return new RecentFeedbackDto
        {
            FeedbackId = feedback.Id,
            InterviewAnswerId = feedback.AnswerEvaluationId,
            FeedbackType = feedback.FeedbackType?.ToString() ?? "UNKNOWN",
            Content = feedback.FeedbackContent,
            Score = feedback.AnswerEvaluation?.OverallScore,
            CreatedAt = feedback.CreatedAt
        };
    }

    public static DashboardDto ToDashboardDto(
        this ReadinessSummaryDto readiness,
        SkillGapSummaryDto skillGaps,
        InterviewSummaryDto interviews,
        RoadmapProgressSummaryDto roadmapProgress,
        List<RecentFeedbackDto> recentFeedbacks)
    {
        return new DashboardDto
        {
            Readiness = readiness,
            SkillGaps = skillGaps,
            Interviews = interviews,
            RoadmapProgress = roadmapProgress,
            RecentFeedbacks = recentFeedbacks,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static decimal CalculateImprovement(ReadinessScore? latest, ReadinessScore? previous)
    {
        if (latest == null || previous == null)
        {
            return 0;
        }

        if (previous.Score == 0)
        {
            return latest.Score > 0 ? 100 : 0;
        }

        return Math.Round(((latest.Score - previous.Score) / previous.Score) * 100, 2);
    }

    private static string DetermineTrend(ReadinessScore? latest, ReadinessScore? previous)
    {
        if (latest == null)
        {
            return "NO_DATA";
        }

        if (previous == null)
        {
            return "NEW";
        }

        var difference = latest.Score - previous.Score;

        return difference switch
        {
            > 5 => "IMPROVING",
            < -5 => "DECLINING",
            _ => "STABLE"
        };
    }

    private static int GetPriority(this Domain.Enum.GapLevel? gapLevel)
    {
        return gapLevel switch
        {
            Domain.Enum.GapLevel.CRITICAL => 1,
            Domain.Enum.GapLevel.HIGH => 2,
            Domain.Enum.GapLevel.MEDIUM => 3,
            Domain.Enum.GapLevel.LOW => 4,
            _ => 5
        };
    }
}
