using AIInterviewPlatform.Application.DTOs.Interview.Enums;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Repositories;

public class InterviewSessionRepository : IInterviewSessionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InterviewSessionRepository> _logger;

    private static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Technical",
        "Behavioral",
        "Communication"
    };

    public InterviewSessionRepository(
        ApplicationDbContext context,
        ILogger<InterviewSessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InterviewSession> CreateSessionWithQuestionsAsync(
        long userId,
        long targetJobId,
        long skillGapAnalysisId,
        List<(string QuestionContent, string Category, string SkillArea)> questions,
        bool isFallback,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _logger.LogInformation(
                "Creating interview session. UserId: {UserId}, TargetJobId: {TargetJobId}, Questions: {Count}",
                userId, targetJobId, questions.Count);

            // Validate all categories exist
            var categoryNames = questions.Select(q => q.Category).Distinct().ToList();
            var existingCategories = await _context.QuestionCategories
                .Where(c => categoryNames.Contains(c.CategoryName))
                .ToListAsync(cancellationToken);

            var existingCategoryNames = existingCategories.Select(c => c.CategoryName.ToLower()).ToHashSet();

            foreach (var categoryName in categoryNames)
            {
                if (!existingCategoryNames.Contains(categoryName.ToLower()))
                {
                    throw new InvalidOperationException(
                        $"Category '{categoryName}' not found. Valid categories are: {string.Join(", ", ValidCategories)}");
                }
            }

            var categoryMap = existingCategories.ToDictionary(
                c => c.CategoryName.ToLower(),
                c => c);

            // Create session with PENDING status
            var session = new InterviewSession
            {
                UserId = userId,
                TargetJobId = targetJobId,
                SessionStatus = SessionStatus.PENDING,
                StartedAt = DateTime.UtcNow
            };

            _context.InterviewSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Interview session created. SessionId: {SessionId}", session.Id);

            // Create questions and link to session
            var questionEntities = new List<InterviewQuestion>();

            foreach (var (questionContent, category, skillArea) in questions)
            {
                var questionCategory = categoryMap[category.ToLower()];

                var question = new InterviewQuestion
                {
                    InterviewSessionId = session.Id,
                    CategoryId = questionCategory.Id,
                    QuestionContent = questionContent,
                    SkillFocus = skillArea,
                    GeneratedBy = isFallback ? QuestionGeneratedBy.TEMPLATE : QuestionGeneratedBy.AI,
                    CreatedAt = DateTime.UtcNow
                };

                questionEntities.Add(question);
            }

            _context.InterviewQuestions.AddRange(questionEntities);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Interview session with questions created. SessionId: {SessionId}, Questions: {Count}",
                session.Id, questionEntities.Count);

            // Reload with navigation properties
            return await GetSessionWithQuestionsAsync(session.Id, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to reload session after creation");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create interview session. UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<InterviewSession?> GetSessionWithQuestionsAsync(
        long sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.InterviewSessions
            .Include(s => s.InterviewQuestions)
                .ThenInclude(q => q.Category)
            .Include(s => s.TargetJob)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<InterviewSession?> GetSessionByIdAsync(
        long sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.InterviewSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<List<InterviewSession>> GetUserSessionsAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.InterviewSessions
            .Include(s => s.InterviewQuestions)
            .Include(s => s.TargetJob)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateSessionStatusAsync(
        long sessionId,
        SessionStatus status,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.InterviewSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning("Session not found for update. SessionId: {SessionId}", sessionId);
            return;
        }

        session.SessionStatus = status;

        if (status == SessionStatus.COMPLETED)
        {
            session.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Session status updated. SessionId: {SessionId}, Status: {Status}",
            sessionId, status);
    }
}
