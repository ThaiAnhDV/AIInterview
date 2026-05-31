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
        return await _context.ReadinessScores
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CalculatedAt)
            .Take(count)
            .ToListAsync();
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
        return await _context.SkillGapAnalyses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SkillGap>> GetSkillGapsByAnalysisIdAsync(long analysisId)
    {
        return await _context.SkillGaps
            .Include(x => x.Skill)
            .Where(x => x.SkillGapAnalysisId == analysisId)
            .ToListAsync();
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

        return scores;
    }
}
