using AIInterviewPlatform.Application.DTOs.Dashboard;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Application.Mappings;
using AIInterviewPlatform.Application.Common.Exceptions;
using AIInterviewPlatform.Application.Common.Validators;

namespace AIInterviewPlatform.Infrastructure.Services;

public class DashboardQueryService : IDashboardQueryService
{
    private readonly IDashboardRepository _repository;
    private readonly IDashboardQueryValidator _validator;

    public DashboardQueryService(
        IDashboardRepository repository,
        IDashboardQueryValidator validator)
    {
        _repository = repository;
        _validator = validator;
        Console.WriteLine("███████████████████████████████████████████████████");
        Console.WriteLine("█ DASHBOARDQUERYSERVICE CONSTRUCTOR CALLED");
        Console.WriteLine("███████████████████████████████████████████████████");
    }

    public async Task<DashboardDto> GetDashboardAsync(long userId, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("███████████████████████████████████████████████████");
        Console.WriteLine("█ DASHBOARDQUERYSERVICE.GetDashboardAsync() CALLED");
        Console.WriteLine($"█ userId: {userId}");
        Console.WriteLine("███████████████████████████████████████████████████");

        var scores = await _repository.GetReadinessScoresAsync(userId, 10);
        var latestScore = scores.FirstOrDefault();
        var previousScore = scores.Skip(1).FirstOrDefault();

        var readinessSummary = latestScore.ToReadinessSummary(previousScore);

        var latestAnalysis = await _repository.GetLatestSkillGapAnalysisAsync(userId);
        var skillGaps = latestAnalysis != null
            ? await _repository.GetSkillGapsByAnalysisIdAsync(latestAnalysis.Id)
            : new List<Domain.Enities.SkillGap>();

        var skillGapSummary = latestAnalysis.ToSkillGapSummary(skillGaps);

        var totalInterviews = await _repository.GetInterviewCountAsync(userId);
        var completedInterviews = await _repository.GetCompletedInterviewCountAsync(userId);
        var avgScore = await _repository.GetAverageInterviewScoreAsync(userId);
        var interviewSummary = new InterviewSummaryDto
        {
            TotalInterviews = totalInterviews,
            CompletedInterviews = completedInterviews,
            PendingInterviews = totalInterviews - completedInterviews,
            AverageScore = avgScore ?? 0
        };

        var roadmaps = await _repository.GetRoadmapsAsync(userId);
        var roadmapProgressSummary = roadmaps.ToRoadmapProgressSummary();

        var feedbacks = await _repository.GetRecentFeedbacksAsync(userId, 10);
        var recentFeedbackDtos = feedbacks.Select(f => f.ToRecentFeedbackDto()).ToList();

        return readinessSummary.ToDashboardDto(
            skillGapSummary,
            interviewSummary,
            roadmapProgressSummary,
            recentFeedbackDtos);
    }
}
