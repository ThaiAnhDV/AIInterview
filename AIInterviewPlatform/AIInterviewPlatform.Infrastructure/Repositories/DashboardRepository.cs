using AIInterviewPlatform.Application.DTOs.Dashboard;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReadinessScore>> GetReadinessScoresAsync(long userId, int count)
    {
        var result = await _context.ReadinessScores
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CalculatedAt)
            .Take(count)
            .ToListAsync();

        Console.WriteLine($"[REPO] GetReadinessScoresAsync - UserId: {userId}, Count returned: {result.Count}");
        foreach (var score in result)
        {
            Console.WriteLine($"  - Score: {score.Score}, CalculatedAt: {score.CalculatedAt}");
        }

        return result;
    }

    public async Task<List<SkillGapAnalysis>> GetSkillGapAnalysesAsync(long userId)
    {
        return await _context.SkillGapAnalyses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<SkillGapAnalysis?> GetLatestSkillGapAnalysisAsync(long userId)
    {
        var result = await _context.SkillGapAnalyses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        Console.WriteLine($"[REPO] GetLatestSkillGapAnalysisAsync - UserId: {userId}, Found: {result != null}, Id: {result?.Id}");

        return result;
    }

    public async Task<List<SkillGap>> GetSkillGapsByAnalysisIdAsync(long analysisId)
    {
        var result = await _context.SkillGaps
            .Include(x => x.Skill)
            .Where(x => x.SkillGapAnalysisId == analysisId)
            .ToListAsync();

        Console.WriteLine($"[REPO] GetSkillGapsByAnalysisIdAsync - AnalysisId: {analysisId}, Count: {result.Count}");

        return result;
    }

    public async Task<List<ReadinessTimelineDto>> GetReadinessTimelineAsync(long userId, int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var scores = await _context.ReadinessScores
            .Where(x => x.UserId == userId && x.CalculatedAt >= startDate)
            .OrderBy(x => x.CalculatedAt)
            .Select(x => new ReadinessTimelineDto
            {
                Date = x.CalculatedAt,
                Score = x.Score
            })
            .ToListAsync();

        Console.WriteLine($"[REPO] GetReadinessTimelineAsync - UserId: {userId}, Days: {days}, Count: {scores.Count}");

        return scores;
    }

    public async Task<List<Feedback>> GetRecentFeedbacksAsync(long userId, int count)
    {
        var result = await _context.Feedbacks
            .Include(x => x.AnswerEvaluation)
                .ThenInclude(a => a.InterviewAnswer)
                    .ThenInclude(ia => ia.InterviewSession)
            .Where(x => x.AnswerEvaluation.InterviewAnswer.InterviewSession.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();

        Console.WriteLine($"[REPO] GetRecentFeedbacksAsync - UserId: {userId}, Count: {result.Count}");

        return result;
    }

    public async Task<int> GetInterviewCountAsync(long userId)
    {
        var count = await _context.InterviewSessions
            .Where(x => x.UserId == userId)
            .CountAsync();

        Console.WriteLine($"[REPO] GetInterviewCountAsync - UserId: {userId}, Count: {count}");

        return count;
    }

    public async Task<int> GetCompletedInterviewCountAsync(long userId)
    {
        var sessions = await _context.InterviewSessions
            .Where(x => x.UserId == userId)
            .ToListAsync();

        Console.WriteLine($"[REPO] All sessions for user {userId}: {sessions.Count}");
        foreach (var s in sessions)
        {
            Console.WriteLine($"  - Session {s.Id}: Status={s.SessionStatus}");
        }

        var count = sessions.Count(x => x.SessionStatus == Domain.Enum.SessionStatus.COMPLETED);

        Console.WriteLine($"[REPO] GetCompletedInterviewCountAsync - UserId: {userId}, Completed: {count}");

        return count;
    }

    public async Task<decimal?> GetAverageInterviewScoreAsync(long userId)
    {
        var avgScore = await _context.AnswerEvaluations
            .Include(x => x.InterviewAnswer)
                .ThenInclude(a => a.InterviewSession)
            .Where(x => x.InterviewAnswer.InterviewSession.UserId == userId && x.OverallScore.HasValue)
            .AverageAsync(x => (decimal?)x.OverallScore);

        Console.WriteLine($"[REPO] GetAverageInterviewScoreAsync - UserId: {userId}, AvgScore: {avgScore}");

        return avgScore;
    }

    public async Task<decimal?> GetHighestInterviewScoreAsync(long userId)
    {
        var highestScore = await _context.AnswerEvaluations
            .Include(x => x.InterviewAnswer)
                .ThenInclude(a => a.InterviewSession)
            .Where(x => x.InterviewAnswer.InterviewSession.UserId == userId && x.OverallScore.HasValue)
            .MaxAsync(x => (decimal?)x.OverallScore);

        Console.WriteLine($"[REPO] GetHighestInterviewScoreAsync - UserId: {userId}, HighestScore: {highestScore}");

        return highestScore;
    }

    public async Task<decimal?> GetLowestInterviewScoreAsync(long userId)
    {
        var lowestScore = await _context.AnswerEvaluations
            .Include(x => x.InterviewAnswer)
                .ThenInclude(a => a.InterviewSession)
            .Where(x => x.InterviewAnswer.InterviewSession.UserId == userId && x.OverallScore.HasValue)
            .MinAsync(x => (decimal?)x.OverallScore);

        Console.WriteLine($"[REPO] GetLowestInterviewScoreAsync - UserId: {userId}, LowestScore: {lowestScore}");

        return lowestScore;
    }

    public async Task<List<LearningRoadmap>> GetRoadmapsAsync(long userId)
    {
        var result = await _context.LearningRoadmaps
            .Include(x => x.RoadmapProgress)
            .Include(x => x.RoadmapMilestones)
            .Where(x => x.UserId == userId)
            .ToListAsync();

        Console.WriteLine($"[REPO] GetRoadmapsAsync - UserId: {userId}, Count: {result.Count}");
        foreach (var r in result)
        {
            Console.WriteLine($"  - Roadmap {r.Id}: Title='{r.RoadmapTitle}', Status={r.RoadmapStatus}, HasProgress={r.RoadmapProgress != null}");
            if (r.RoadmapProgress != null)
            {
                Console.WriteLine($"    - CompletionPercentage: {r.RoadmapProgress.CompletionPercentage}");
            }
        }

        return result;
    }

    public async Task<RoadmapProgress?> GetRoadmapProgressAsync(long roadmapId)
    {
        return await _context.RoadmapProgresses
            .Where(x => x.LearningRoadmapId == roadmapId)
            .FirstOrDefaultAsync();
    }
}
