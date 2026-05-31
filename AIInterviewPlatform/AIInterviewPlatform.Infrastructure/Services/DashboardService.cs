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
}
