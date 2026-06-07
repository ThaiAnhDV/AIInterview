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
        string? languageCode = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating answer {AnswerId}", answerId);
        _logger.LogInformation("[Evaluation] PIPELINE=GEMINI AnswerId={AnswerId}", answerId);

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

            var existingEvaluation = await LoadPersistedEvaluationAsync(answerId, cancellationToken);
            return existingEvaluation ?? CreateFailure(
                "ALREADY_EVALUATED",
                $"Answer {answerId} has already been evaluated.",
                "Answer already has a persisted evaluation.");
        }

        var request = BuildEvaluationRequest(questionLoad.Question!, answerLoad.Answer, languageCode);
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
        string? languageCode = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating answer {AnswerId} for user {UserId}", answerId, userId);
        _logger.LogInformation("[Evaluation] PIPELINE=GEMINI AnswerId={AnswerId} UserId={UserId}", answerId, userId);

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

            var existingEvaluation = await LoadPersistedEvaluationAsync(answerId, cancellationToken);
            return existingEvaluation ?? CreateFailure(
                "ALREADY_EVALUATED",
                $"Answer {answerId} has already been evaluated.",
                "Answer already has a persisted evaluation.");
        }

        var request = BuildEvaluationRequest(questionLoad.Question!, answerLoad.Answer, languageCode);
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
            _logger.LogWarning(
                "Skipping persistence for answer {AnswerId}. Success={Success}, ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}",
                answerId,
                result.Success,
                result.ErrorCode,
                result.ErrorMessage);
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var evaluation = new AnswerEvaluation
            {
                InterviewAnswerId = answerId,
                ClarityScore = result.Clarity,
                StructureScore = result.TechnicalAccuracy,
                RelevanceScore = result.Completeness,
                OverallScore = result.Overall,
                EvaluatedAt = DateTime.UtcNow
            };

            var feedback = new Feedback
            {
                FeedbackContent = result.Feedback,
                FeedbackType = FeedbackType.OVERALL,
                CreatedAt = DateTime.UtcNow
            };

            var weaknesses = result.Weaknesses.Any()
                ? string.Join(" | ", result.Weaknesses)
                : "No weaknesses provided.";

            var improvement = new DomainImprovement
            {
                SuggestionContent = weaknesses,
                PriorityLevel = DeterminePriority(result.Overall)
            };

            evaluation.Feedbacks.Add(feedback);
            feedback.ImprovementSuggestions.Add(improvement);

            _logger.LogInformation(
                "[Evaluation] PIPELINE=GEMINI Insert Feedback AnswerId={AnswerId} FeedbackType={FeedbackType} Content={FeedbackContent}",
                answerId,
                feedback.FeedbackType,
                feedback.FeedbackContent);
            _logger.LogInformation(
                "[Evaluation] PIPELINE=GEMINI Insert ImprovementSuggestion AnswerId={AnswerId} Priority={Priority} Content={SuggestionContent}",
                answerId,
                improvement.PriorityLevel,
                improvement.SuggestionContent);

            _context.AnswerEvaluations.Add(evaluation);
            _logger.LogInformation(
                "[Evaluation] PIPELINE=GEMINI Insert AnswerEvaluation AnswerId={AnswerId} Clarity={Clarity} TechnicalAccuracy={TechnicalAccuracy} Completeness={Completeness} Overall={Overall}",
                answerId,
                result.Clarity,
                result.TechnicalAccuracy,
                result.Completeness,
                result.Overall);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Persisted evaluation for answer {AnswerId}: AIOverall={Overall}, StoredOverall={StoredOverall}, EvaluationId={EvaluationId}",
                answerId,
                result.Overall,
                evaluation.OverallScore,
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

    private async Task<EvaluationResultDto?> LoadPersistedEvaluationAsync(
        long answerId,
        CancellationToken cancellationToken)
    {
        var evaluation = await _context.AnswerEvaluations
            .Include(e => e.Feedbacks)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.InterviewAnswerId == answerId, cancellationToken);

        if (evaluation == null)
        {
            return null;
        }

        var overallFeedback = evaluation.Feedbacks
            .Where(f => f.FeedbackType == FeedbackType.OVERALL)
            .Select(f => f.FeedbackContent)
            .FirstOrDefault();

        return new EvaluationResultDto
        {
            Success = true,
            AiUsed = true,
            GeneratedBy = "GEMINI",
            IsFallback = false,
            Clarity = evaluation.ClarityScore ?? 0,
            TechnicalAccuracy = evaluation.StructureScore ?? 0,
            Completeness = evaluation.RelevanceScore ?? 0,
            Overall = evaluation.OverallScore ?? 0,
            Feedback = overallFeedback ?? string.Join(" ", evaluation.Feedbacks.Select(f => f.FeedbackContent).Where(content => !string.IsNullOrWhiteSpace(content))),
            Strengths = new List<string>(),
            Weaknesses = new List<string>(),
            Message = "Evaluation already exists."
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
        InterviewAnswer answer,
        string? languageCode)
    {
        return new EvaluationRequestDto
        {
            Question = question.QuestionContent,
            Answer = answer.AnswerText,
            Category = question.Category?.CategoryName,
            SkillFocus = question.SkillFocus,
            LanguageCode = languageCode
        };
    }

    private static EvaluationResultDto CreateFailure(string errorCode, string message, string? errorMessage = null)
    {
        return new EvaluationResultDto
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            ErrorMessage = errorMessage ?? message,
            AiUsed = false,
            GeneratedBy = "FAILED",
            IsFallback = false,
            Feedback = message,
            Strengths = new List<string>(),
            Weaknesses = new List<string>()
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
