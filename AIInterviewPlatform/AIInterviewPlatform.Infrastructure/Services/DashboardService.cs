using AIInterviewPlatform.Application.DTOs.Dashboard;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;

using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        ILogger<DashboardService> logger)
    {
        _dashboardRepository = dashboardRepository;
        _logger = logger;
        Console.WriteLine("███████████████████████████████████████████████████");
        Console.WriteLine("█ DASHBOARDSERVICE CONSTRUCTOR CALLED");
        Console.WriteLine("███████████████████████████████████████████████████");
    }

    public async Task<ReadinessDashboardResponse> GetReadinessDashboardAsync(long userId)
    {
        _logger.LogInformation("Getting readiness dashboard for user {UserId}", userId);

        var scores = await _dashboardRepository.GetReadinessScoresAsync(userId, 10);

        var latestScore = scores.FirstOrDefault();
        var previousScore = scores.Skip(1).FirstOrDefault();

        var improvementPercentage = CalculateImprovementPercentage(latestScore, previousScore);
        var trend = DetermineTrend(latestScore, previousScore);

        return new ReadinessDashboardResponse
        {
            LatestScore = latestScore != null
                ? new ReadinessScoreDto
                {
                    Score = latestScore.Score,
                    CalculatedAt = latestScore.CalculatedAt
                }
                : null,
            PreviousScore = previousScore != null
                ? new ReadinessScoreDto
                {
                    Score = previousScore.Score,
                    CalculatedAt = previousScore.CalculatedAt
                }
                : null,
            ImprovementPercentage = improvementPercentage,
            Trend = trend
        };
    }

    public async Task<SkillGapsDashboardResponse?> GetSkillGapsDashboardAsync(long userId)
    {
        _logger.LogInformation("Getting skill gaps dashboard for user {UserId}", userId);

        var latestAnalysis = await _dashboardRepository.GetLatestSkillGapAnalysisAsync(userId);

        if (latestAnalysis == null)
        {
            return null;
        }

        var skillGaps = await _dashboardRepository.GetSkillGapsByAnalysisIdAsync(latestAnalysis.Id);

        return new SkillGapsDashboardResponse
        {
            LatestAnalysisId = latestAnalysis.Id,
            AnalysisDate = latestAnalysis.CreatedAt,
            TotalMissingSkills = skillGaps.Count,
            MissingSkills = skillGaps.Select(gap => new SkillGapDto
            {
                SkillId = gap.SkillId,
                SkillName = gap.Skill.SkillName,
                GapLevel = gap.GapLevel?.ToString() ?? "UNKNOWN",
                GapDescription = gap.GapDescription,
                AnalysisDate = gap.SkillGapAnalysis.CreatedAt
            }).ToList()
        };
    }

    public async Task<HistoryDashboardResponse> GetHistoryDashboardAsync(long userId)
    {
        _logger.LogInformation("Getting history dashboard for user {UserId}", userId);

        var analyses = await _dashboardRepository.GetSkillGapAnalysesAsync(userId);
        var readinessTimeline = await _dashboardRepository.GetReadinessTimelineAsync(userId, 30);

        var analysisHistoryDtos = new List<AnalysisHistoryDto>();

        foreach (var analysis in analyses)
        {
            var score = await GetScoreForAnalysisAsync(analysis.Id, userId);
            var skillGaps = await _dashboardRepository.GetSkillGapsByAnalysisIdAsync(analysis.Id);

            analysisHistoryDtos.Add(new AnalysisHistoryDto
            {
                AnalysisId = analysis.Id,
                ResumeId = analysis.ResumeId,
                JobDescriptionId = analysis.JobDescriptionId,
                ReadinessScore = score,
                MatchedSkillsCount = 0,
                MissingSkillsCount = skillGaps.Count,
                CreatedAt = analysis.CreatedAt
            });
        }

        var allScores = analysisHistoryDtos.Select(x => x.ReadinessScore).ToList();

        return new HistoryDashboardResponse
        {
            Analyses = analysisHistoryDtos,
            ReadinessTimeline = readinessTimeline,
            AverageScore = allScores.Count > 0 ? Math.Round(allScores.Average(), 2) : 0,
            HighestScore = allScores.Count > 0 ? allScores.Max() : 0,
            LowestScore = allScores.Count > 0 ? allScores.Min() : 0,
            TotalAnalyses = analyses.Count
        };
    }

    private async Task<decimal> GetScoreForAnalysisAsync(long analysisId, long userId)
    {
        var scores = await _dashboardRepository.GetReadinessScoresAsync(userId, 100);
        var score = scores.FirstOrDefault(x => x.SkillGapAnalysisId == analysisId);
        return score?.Score ?? 0;
    }

    private static decimal CalculateImprovementPercentage(
        ReadinessScore? latest,
        ReadinessScore? previous)
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

    public async Task<DashboardDto> GetDashboardAsync(long userId)
    {
        Console.WriteLine("███████████████████████████████████████████████████");
        Console.WriteLine("█ DASHBOARDSERVICE.GetDashboardAsync() CALLED");
        Console.WriteLine($"█ userId: {userId}");
        Console.WriteLine("███████████████████████████████████████████████████");

        var readiness = await GetReadinessDashboardAsync(userId);
        _logger.LogInformation("Readiness - LatestScore: {LatestScore}, PreviousScore: {PreviousScore}, Trend: {Trend}",
            readiness.LatestScore?.Score,
            readiness.PreviousScore?.Score,
            readiness.Trend);

        var skillGaps = await GetSkillGapsDashboardAsync(userId);
        _logger.LogInformation("SkillGaps - TotalMissingSkills: {Total}, HighPriority: {High}, Medium: {Medium}, Low: {Low}",
            skillGaps?.TotalMissingSkills ?? 0,
            skillGaps?.MissingSkills?.Count(x => x.GapLevel == "HIGH" || x.GapLevel == "CRITICAL") ?? 0,
            skillGaps?.MissingSkills?.Count(x => x.GapLevel == "MEDIUM") ?? 0,
            skillGaps?.MissingSkills?.Count(x => x.GapLevel == "LOW") ?? 0);

        var feedbacks = await _dashboardRepository.GetRecentFeedbacksAsync(userId, 10);
        _logger.LogInformation("Feedbacks count: {Count}", feedbacks?.Count ?? 0);

        var interviewCount = await _dashboardRepository.GetInterviewCountAsync(userId);
        _logger.LogInformation("Interview count: {Count}", interviewCount);

        var completedCount = await _dashboardRepository.GetCompletedInterviewCountAsync(userId);
        _logger.LogInformation("Completed interviews: {Completed}", completedCount);

        var avgScore = await _dashboardRepository.GetAverageInterviewScoreAsync(userId);
        _logger.LogInformation("Average interview score: {AvgScore}", avgScore);

        var highestScore = await _dashboardRepository.GetHighestInterviewScoreAsync(userId);
        _logger.LogInformation("Highest interview score: {HighestScore}", highestScore);

        var lowestScore = await _dashboardRepository.GetLowestInterviewScoreAsync(userId);
        _logger.LogInformation("Lowest interview score: {LowestScore}", lowestScore);

        var roadmaps = await _dashboardRepository.GetRoadmapsAsync(userId);
        _logger.LogInformation("Roadmaps count: {Count}", roadmaps?.Count ?? 0);

        var roadmapProgressSummary = CalculateRoadmapProgress(roadmaps ?? new List<LearningRoadmap>());
        _logger.LogInformation("Roadmap Progress - TotalRoadmaps: {Total}, OverallProgress: {Progress}%, ActiveRoadmap: {Active}",
            roadmapProgressSummary.TotalRoadmaps,
            roadmapProgressSummary.OverallProgressPercentage,
            roadmapProgressSummary.ActiveRoadmapTitle ?? "N/A");

        _logger.LogInformation("=== BUILDING DASHBOARD DTO ===");

        return new DashboardDto
        {
            Readiness = new ReadinessSummaryDto
            {
                CurrentScore = readiness.LatestScore?.Score,
                PreviousScore = readiness.PreviousScore?.Score,
                ImprovementPercentage = readiness.ImprovementPercentage,
                Trend = readiness.Trend,
                CalculatedAt = readiness.LatestScore?.CalculatedAt
            },
            SkillGaps = skillGaps != null
                ? new SkillGapSummaryDto
                {
                    TotalMissingSkills = skillGaps.TotalMissingSkills,
                    HighPriorityCount = skillGaps.MissingSkills.Count(x => x.GapLevel == "HIGH" || x.GapLevel == "CRITICAL"),
                    MediumPriorityCount = skillGaps.MissingSkills.Count(x => x.GapLevel == "MEDIUM"),
                    LowPriorityCount = skillGaps.MissingSkills.Count(x => x.GapLevel == "LOW"),
                    MissingSkills = skillGaps.MissingSkills.Select(g => new MissingSkillDto
                    {
                        SkillId = g.SkillId,
                        SkillName = g.SkillName,
                        GapLevel = g.GapLevel,
                        GapDescription = g.GapDescription,
                        Priority = GetPriorityFromGapLevel(g.GapLevel)
                    }).ToList(),
                    LastAnalyzedAt = skillGaps.AnalysisDate
                }
                : new SkillGapSummaryDto(),
            Interviews = new InterviewSummaryDto
            {
                TotalInterviews = interviewCount,
                CompletedInterviews = completedCount,
                PendingInterviews = interviewCount - completedCount,
                AverageScore = avgScore ?? 0,
                HighestScore = highestScore,
                LowestScore = lowestScore
            },
            RoadmapProgress = roadmapProgressSummary,
            RecentFeedbacks = feedbacks.Select(f => new RecentFeedbackDto
            {
                FeedbackId = f.Id,
                InterviewAnswerId = f.AnswerEvaluationId,
                FeedbackType = f.FeedbackType?.ToString() ?? "UNKNOWN",
                Content = f.FeedbackContent,
                Score = f.AnswerEvaluation.OverallScore,
                CreatedAt = f.CreatedAt
            }).ToList(),
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static int GetPriorityFromGapLevel(string gapLevel)
    {
        return gapLevel.ToUpperInvariant() switch
        {
            "CRITICAL" => 1,
            "HIGH" => 2,
            "MEDIUM" => 3,
            "LOW" => 4,
            _ => 5
        };
    }

    private static RoadmapProgressSummaryDto CalculateRoadmapProgress(List<LearningRoadmap> roadmaps)
    {
        Console.WriteLine($"[SERVICE] CalculateRoadmapProgress called with {roadmaps.Count} roadmaps");

        var summary = new RoadmapProgressSummaryDto
        {
            TotalRoadmaps = roadmaps.Count
        };

        if (roadmaps.Count == 0)
        {
            Console.WriteLine("[SERVICE] No roadmaps found, returning empty summary");
            return summary;
        }

        foreach (var roadmap in roadmaps)
        {
            Console.WriteLine($"[SERVICE] Roadmap: {roadmap.RoadmapTitle}, Status: {roadmap.RoadmapStatus}, HasProgress: {roadmap.RoadmapProgress != null}");
            if (roadmap.RoadmapProgress != null)
            {
                Console.WriteLine($"[SERVICE]   - CompletionPercentage: {roadmap.RoadmapProgress.CompletionPercentage}");
            }
        }

        var activeRoadmap = roadmaps
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefault(r => r.RoadmapStatus == Domain.Enum.RoadmapStatus.ACTIVE);

        if (activeRoadmap != null)
        {
            Console.WriteLine($"[SERVICE] Active roadmap found: {activeRoadmap.RoadmapTitle}");
            summary.ActiveRoadmapTitle = activeRoadmap.RoadmapTitle;
            summary.ActiveRoadmapProgress = activeRoadmap.RoadmapProgress?.CompletionPercentage ?? 0;
            Console.WriteLine($"[SERVICE] ActiveRoadmapProgress: {summary.ActiveRoadmapProgress}");
        }
        else
        {
            Console.WriteLine("[SERVICE] No ACTIVE roadmap found");
        }

        var allMilestones = roadmaps.SelectMany(r => r.RoadmapMilestones).ToList();
        summary.TotalMilestones = allMilestones.Count;
        summary.CompletedMilestones = allMilestones.Count(m => m.IsCompleted);

        var progressValues = roadmaps
            .Where(r => r.RoadmapProgress != null)
            .Select(r => r.RoadmapProgress!.CompletionPercentage)
            .ToList();

        Console.WriteLine($"[SERVICE] progressValues count: {progressValues.Count}, values: [{string.Join(", ", progressValues)}]");

        summary.OverallProgressPercentage = progressValues.Count > 0
            ? Math.Round(progressValues.Average(), 2)
            : 0;

        Console.WriteLine($"[SERVICE] OverallProgressPercentage: {summary.OverallProgressPercentage}");

        return summary;
    }
}
