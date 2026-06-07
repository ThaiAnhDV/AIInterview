using AIInterviewPlatform.Application.DTOs.AnswerEvaluation;
using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class AnswerEvaluationService : IAnswerEvaluationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInterviewEvaluationApplicationService _geminiEvaluationService;
        private readonly ILogger<AnswerEvaluationService> _logger;

        public AnswerEvaluationService(
            ApplicationDbContext context,
            IInterviewEvaluationApplicationService geminiEvaluationService,
            ILogger<AnswerEvaluationService> logger)
        {
            _context = context;
            _geminiEvaluationService = geminiEvaluationService;
            _logger = logger;
        }

        public async Task<AnswerEvaluationResponse> EvaluateAnswerAsync(long answerId)
        {
            _logger.LogInformation("[Evaluation] PIPELINE=GEMINI_VIA_LEGACY_CONTROLLER AnswerId={AnswerId}", answerId);

            var evaluationResult = await _geminiEvaluationService.EvaluateAnswerAsync(answerId);
            if (!evaluationResult.Success)
            {
                return new AnswerEvaluationResponse
                {
                    Success = false,
                    ErrorCode = evaluationResult.ErrorCode,
                    Message = evaluationResult.ErrorMessage ?? evaluationResult.Message ?? "Gemini evaluation failed.",
                    InterviewAnswerId = answerId
                };
            }

            return await GetEvaluationAsync(answerId)
                   ?? new AnswerEvaluationResponse
                   {
                       Success = false,
                       ErrorCode = "EVALUATION_NOT_FOUND",
                       Message = "Gemini evaluation completed but persisted evaluation could not be loaded.",
                       InterviewAnswerId = answerId
                   };
        }

        public async Task<AnswerEvaluationResponse?> GetEvaluationAsync(long answerId)
        {
            var evaluation = await _context.AnswerEvaluations
                .Include(x => x.Feedbacks)
                .FirstOrDefaultAsync(x =>
                    x.InterviewAnswerId == answerId);

            if (evaluation == null)
                return null;

            return new AnswerEvaluationResponse
            {
                Success = true,
                Id = evaluation.Id,
                InterviewAnswerId = evaluation.InterviewAnswerId,
                ClarityScore = evaluation.ClarityScore ?? 0,
                StructureScore = evaluation.StructureScore ?? 0,
                RelevanceScore = evaluation.RelevanceScore ?? 0,
                OverallScore = evaluation.OverallScore ?? 0,
                EvaluatedAt = evaluation.EvaluatedAt,
                Feedbacks = evaluation.Feedbacks
                    .Select(f => new FeedbackResponse
                    {
                        Id = f.Id,
                        FeedbackContent = f.FeedbackContent,
                        FeedbackType = f.FeedbackType.ToString(),
                        CreatedAt = f.CreatedAt
                    })
                    .ToList()
            };
        }

        public async Task<SessionFeedbackResponse> GetSessionFeedbackAsync(long sessionId)
        {
            var evaluations = await _context.AnswerEvaluations
                .Include(x => x.Feedbacks)
                .Include(x => x.InterviewAnswer)
                .Where(x =>
                    x.InterviewAnswer.InterviewSessionId
                    == sessionId)
                .ToListAsync();

            return new SessionFeedbackResponse
            {
                InterviewSessionId = sessionId,
                AverageScore = evaluations.Any()
                    ? evaluations.Average(x =>
                        x.OverallScore ?? 0)
                    : 0,
                Evaluations = evaluations.Select(e =>
                    new AnswerEvaluationResponse
                    {
                        Success = true,
                        Id = e.Id,
                        InterviewAnswerId =
                            e.InterviewAnswerId,
                        ClarityScore =
                            e.ClarityScore ?? 0,
                        StructureScore =
                            e.StructureScore ?? 0,
                        RelevanceScore =
                            e.RelevanceScore ?? 0,
                        OverallScore =
                            e.OverallScore ?? 0,
                        EvaluatedAt =
                            e.EvaluatedAt,
                        Feedbacks =
                            e.Feedbacks.Select(f =>
                                new FeedbackResponse
                                {
                                    Id = f.Id,
                                    FeedbackContent =
                                        f.FeedbackContent,
                                    FeedbackType =
                                        f.FeedbackType
                                            .ToString(),
                                    CreatedAt =
                                        f.CreatedAt
                                })
                                .ToList()
                    })
                    .ToList()
            };
        }
    }
}
