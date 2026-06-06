using AIInterviewPlatform.Application.DTOs.AnswerEvaluation;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class AnswerEvaluationService : IAnswerEvaluationService
    {
        private readonly ApplicationDbContext _context;

        public AnswerEvaluationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AnswerEvaluationResponse> EvaluateAnswerAsync(long answerId)
        {
            var answer = await _context.InterviewAnswers
                .Include(x => x.AnswerEvaluation)
                .FirstOrDefaultAsync(x => x.Id == answerId);

            if (answer == null)
                throw new Exception("Answer not found");

            if (answer.AnswerEvaluation != null)
            {
                return await GetEvaluationAsync(answerId)
                       ?? throw new Exception();
            }

            decimal clarity = 70;
            decimal structure = 75;
            decimal relevance = 80;

            if (answer.AnswerText.Length > 100)
                clarity += 10;

            if (answer.AnswerText.Length > 200)
                structure += 10;

            decimal overall =
                (clarity + structure + relevance) / 3;

            var evaluation = new AnswerEvaluation
            {
                InterviewAnswerId = answerId,
                ClarityScore = clarity,
                StructureScore = structure,
                RelevanceScore = relevance,
                OverallScore = overall
            };

            _context.AnswerEvaluations.Add(evaluation);

            await _context.SaveChangesAsync();

            _context.Feedbacks.AddRange(
                new Feedback
                {
                    AnswerEvaluationId = evaluation.Id,
                    FeedbackType = FeedbackType.CLARITY,
                    FeedbackContent =
                        "Answer clarity is acceptable."
                },
                new Feedback
                {
                    AnswerEvaluationId = evaluation.Id,
                    FeedbackType = FeedbackType.STRUCTURE,
                    FeedbackContent =
                        "Answer structure is good."
                },
                new Feedback
                {
                    AnswerEvaluationId = evaluation.Id,
                    FeedbackType = FeedbackType.RELEVANCE,
                    FeedbackContent =
                        "Answer is relevant to the question."
                });

            await _context.SaveChangesAsync();

            return await GetEvaluationAsync(answerId)
                   ?? throw new Exception();
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