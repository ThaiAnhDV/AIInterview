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

        var answerLoad = await LoadInterviewAnswerAsync(answerId, cancellationToken);
        if (!answerLoad.Success)
        {
            return answerLoad.Error!;
        }

        var questionLoad = await LoadInterviewQuestionAsync(answerLoad.Answer!.InterviewQuestionId, cancellationToken);
        if (!questionLoad.Success)
        {
            return questionLoad.Error!;
        }

        if (await HasExistingEvaluationAsync(answerId, cancellationToken))
        {
            _logger.LogWarning("Answer {AnswerId} already has an evaluation", answerId);
            return CreateFailure("ALREADY_EVALUATED", $"Answer {answerId} has already been evaluated.");
        }

        var request = BuildEvaluationRequest(questionLoad.Question!, answerLoad.Answer);
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

        var answerLoad = await LoadInterviewAnswerWithUserValidationAsync(answerId, userId, cancellationToken);
        if (!answerLoad.Success)
        {
            return answerLoad.Error!;
        }

        var questionLoad = await LoadInterviewQuestionAsync(answerLoad.Answer!.InterviewQuestionId, cancellationToken);
        if (!questionLoad.Success)
        {
            return questionLoad.Error!;
        }

        if (await HasExistingEvaluationAsync(answerId, cancellationToken))
        {
            _logger.LogWarning("Answer {AnswerId} already has an evaluation", answerId);
            return CreateFailure("ALREADY_EVALUATED", $"Answer {answerId} has already been evaluated.");
        }

        var request = BuildEvaluationRequest(questionLoad.Question!, answerLoad.Answer);
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
        if (!result.Success)
        {
            return;
        }

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

    private async Task<AnswerLoadResult> LoadInterviewAnswerAsync(
        long answerId,
        CancellationToken cancellationToken)
    {
        var answer = await _context.InterviewAnswers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer == null)
        {
            _logger.LogWarning("Interview answer {AnswerId} not found", answerId);
            return AnswerLoadResult.Fail(CreateFailure("ANSWER_NOT_FOUND", $"Interview answer with ID {answerId} not found."));
        }

        return AnswerLoadResult.Ok(answer);
    }

    private async Task<AnswerLoadResult> LoadInterviewAnswerWithUserValidationAsync(
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
            return AnswerLoadResult.Fail(CreateFailure("ANSWER_NOT_FOUND", $"Interview answer with ID {answerId} not found."));
        }

        if (answer.InterviewSession.UserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access answer {AnswerId} belonging to another user",
                userId,
                answerId);
            return AnswerLoadResult.Fail(CreateFailure("UNAUTHORIZED", "You do not have permission to access this answer."));
        }

        return AnswerLoadResult.Ok(answer);
    }

    private async Task<QuestionLoadResult> LoadInterviewQuestionAsync(
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
            return QuestionLoadResult.Fail(CreateFailure("QUESTION_NOT_FOUND", $"Interview question with ID {questionId} not found."));
        }

        return QuestionLoadResult.Ok(question);
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

    private static EvaluationResultDto CreateFailure(string errorCode, string message)
    {
        return new EvaluationResultDto
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            Feedback = message,
            Improvement = string.Empty
        };
    }

    private sealed class AnswerLoadResult
    {
        public bool Success { get; init; }
        public InterviewAnswer? Answer { get; init; }
        public EvaluationResultDto? Error { get; init; }

        public static AnswerLoadResult Ok(InterviewAnswer answer) => new() { Success = true, Answer = answer };
        public static AnswerLoadResult Fail(EvaluationResultDto error) => new() { Success = false, Error = error };
    }

    private sealed class QuestionLoadResult
    {
        public bool Success { get; init; }
        public InterviewQuestion? Question { get; init; }
        public EvaluationResultDto? Error { get; init; }

        public static QuestionLoadResult Ok(InterviewQuestion question) => new() { Success = true, Question = question };
        public static QuestionLoadResult Fail(EvaluationResultDto error) => new() { Success = false, Error = error };
    }
}
