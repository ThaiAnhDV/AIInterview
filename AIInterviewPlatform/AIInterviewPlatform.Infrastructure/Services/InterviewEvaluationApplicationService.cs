using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using DomainImprovement = AIInterviewPlatform.Domain.Enities.ImprovementSuggestion;

namespace AIInterviewPlatform.Infrastructure.Services;

public class InterviewEvaluationApplicationService : IInterviewEvaluationApplicationService
{
    private readonly ApplicationDbContext _context;
    private readonly IInterviewEvaluationService _evaluationService;
    private readonly ILogger<InterviewEvaluationApplicationService> _logger;

    public InterviewEvaluationApplicationService(
        ApplicationDbContext context,
        IInterviewEvaluationService evaluationService,
        ILogger<InterviewEvaluationApplicationService> logger)
    {
        _context = context;
        _evaluationService = evaluationService;
        _logger = logger;
    }

    public async Task<EvaluationResultDto> EvaluateAnswerAsync(
        long answerId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating answer {AnswerId}", answerId);

        var answer = await LoadInterviewAnswerAsync(answerId, cancellationToken);
        var question = await LoadInterviewQuestionAsync(answer.InterviewQuestionId, cancellationToken);

        if (await HasExistingEvaluationAsync(answerId, cancellationToken))
        {
            _logger.LogWarning("Answer {AnswerId} already has an evaluation", answerId);
            throw new InvalidOperationException($"Answer {answerId} has already been evaluated.");
        }

        var request = BuildEvaluationRequest(question, answer);
        var result = await _evaluationService.EvaluateAnswerAsync(request, cancellationToken);

        await PersistEvaluationAsync(answerId, result, cancellationToken);

        _logger.LogInformation(
            "Evaluation completed for answer {AnswerId}. Overall score: {Score}",
            answerId,
            result.Overall);

        return result;
    }

    public async Task<EvaluationResultDto> EvaluateAnswerAsync(
        long userId,
        long answerId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating answer {AnswerId} for user {UserId}", answerId, userId);

        var answer = await LoadInterviewAnswerWithUserValidationAsync(answerId, userId, cancellationToken);
        var question = await LoadInterviewQuestionAsync(answer.InterviewQuestionId, cancellationToken);

        if (await HasExistingEvaluationAsync(answerId, cancellationToken))
        {
            _logger.LogWarning("Answer {AnswerId} already has an evaluation", answerId);
            throw new InvalidOperationException($"Answer {answerId} has already been evaluated.");
        }

        var request = BuildEvaluationRequest(question, answer);
        var result = await _evaluationService.EvaluateAnswerAsync(request, cancellationToken);

        await PersistEvaluationAsync(answerId, result, cancellationToken);

        _logger.LogInformation(
            "Evaluation completed for answer {AnswerId} (user {UserId}). Overall score: {Score}",
            answerId,
            userId,
            result.Overall);

        return result;
    }

    private async Task<bool> HasExistingEvaluationAsync(
        long answerId,
        CancellationToken cancellationToken)
    {
        return await _context.AnswerEvaluations
            .AsNoTracking()
            .AnyAsync(e => e.InterviewAnswerId == answerId, cancellationToken);
    }

    private async Task PersistEvaluationAsync(
        long answerId,
        EvaluationResultDto result,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var evaluation = new AnswerEvaluation
            {
                InterviewAnswerId = answerId,
                ClarityScore = result.Clarity,
                StructureScore = result.Structure,
                RelevanceScore = result.Relevance,
                OverallScore = result.Overall,
                EvaluatedAt = DateTime.UtcNow
            };

            var feedback = new Feedback
            {
                FeedbackContent = result.Feedback,
                FeedbackType = FeedbackType.OVERALL,
                CreatedAt = DateTime.UtcNow
            };

            var improvement = new DomainImprovement
            {
                SuggestionContent = result.Improvement,
                PriorityLevel = DeterminePriority(result.Overall)
            };

            evaluation.Feedbacks.Add(feedback);
            feedback.ImprovementSuggestions.Add(improvement);

            _context.AnswerEvaluations.Add(evaluation);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Evaluation persisted for answer {AnswerId}. EvaluationId: {EvaluationId}",
                answerId,
                evaluation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist evaluation for answer {AnswerId}", answerId);
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    private static PriorityLevel DeterminePriority(decimal overallScore)
    {
        return overallScore switch
        {
            >= 80 => PriorityLevel.LOW,
            >= 60 => PriorityLevel.MEDIUM,
            _ => PriorityLevel.HIGH
        };
    }

    private async Task<InterviewAnswer> LoadInterviewAnswerAsync(
        long answerId,
        CancellationToken cancellationToken)
    {
        var answer = await _context.InterviewAnswers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer == null)
        {
            _logger.LogWarning("Interview answer {AnswerId} not found", answerId);
            throw new InvalidOperationException($"Interview answer with ID {answerId} not found.");
        }

        return answer;
    }

    private async Task<InterviewAnswer> LoadInterviewAnswerWithUserValidationAsync(
        long answerId,
        long userId,
        CancellationToken cancellationToken)
    {
        var answer = await _context.InterviewAnswers
            .Include(a => a.InterviewSession)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer == null)
        {
            _logger.LogWarning("Interview answer {AnswerId} not found", answerId);
            throw new InvalidOperationException($"Interview answer with ID {answerId} not found.");
        }

        if (answer.InterviewSession.UserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access answer {AnswerId} belonging to another user",
                userId,
                answerId);
            throw new UnauthorizedAccessException("You do not have permission to access this answer.");
        }

        return answer;
    }

    private async Task<InterviewQuestion> LoadInterviewQuestionAsync(
        long questionId,
        CancellationToken cancellationToken)
    {
        var question = await _context.InterviewQuestions
            .Include(q => q.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question == null)
        {
            _logger.LogWarning("Interview question {QuestionId} not found", questionId);
            throw new InvalidOperationException($"Interview question with ID {questionId} not found.");
        }

        return question;
    }

    private static EvaluationRequestDto BuildEvaluationRequest(
        InterviewQuestion question,
        InterviewAnswer answer)
    {
        return new EvaluationRequestDto
        {
            Question = question.QuestionContent,
            Answer = answer.AnswerText,
            Category = question.Category?.CategoryName,
            SkillFocus = question.SkillFocus
        };
    }
}
