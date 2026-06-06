using AIInterviewPlatform.Application.DTOs.InterviewAnswer;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class InterviewAnswerService : IInterviewAnswerService
    {
        private readonly ApplicationDbContext _context;

        public InterviewAnswerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InterviewAnswerResponse> SubmitAnswerAsync(
            long userId,
            long sessionId,
            SubmitInterviewAnswerRequest request)
        {
            var session = await _context.InterviewSessions
                .FirstOrDefaultAsync(x =>
                    x.Id == sessionId &&
                    x.UserId == userId);

            if (session == null)
            {
                throw new Exception("Interview session not found.");
            }

            if (session.SessionStatus == SessionStatus.COMPLETED)
            {
                throw new Exception("Cannot submit answer to completed session.");
            }

            var question = await _context.InterviewQuestions
                .FirstOrDefaultAsync(x =>
                    x.Id == request.InterviewQuestionId &&
                    x.InterviewSessionId == sessionId);

            if (question == null)
            {
                throw new Exception("Question not found in this session.");
            }

            var existedAnswer = await _context.InterviewAnswers
                .FirstOrDefaultAsync(x =>
                    x.InterviewSessionId == sessionId &&
                    x.InterviewQuestionId == request.InterviewQuestionId);

            if (existedAnswer != null)
            {
                existedAnswer.AnswerText = request.AnswerText;
                existedAnswer.SubmittedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return new InterviewAnswerResponse
                {
                    Id = existedAnswer.Id,
                    InterviewSessionId = existedAnswer.InterviewSessionId,
                    InterviewQuestionId = existedAnswer.InterviewQuestionId,
                    QuestionContent = question.QuestionContent,
                    AnswerText = existedAnswer.AnswerText,
                    SubmittedAt = existedAnswer.SubmittedAt
                };
            }

            var answer = new InterviewAnswer
            {
                InterviewSessionId = sessionId,
                InterviewQuestionId = request.InterviewQuestionId,
                AnswerText = request.AnswerText,
                SubmittedAt = DateTime.Now
            };

            _context.InterviewAnswers.Add(answer);
            await _context.SaveChangesAsync();

            return new InterviewAnswerResponse
            {
                Id = answer.Id,
                InterviewSessionId = answer.InterviewSessionId,
                InterviewQuestionId = answer.InterviewQuestionId,
                QuestionContent = question.QuestionContent,
                AnswerText = answer.AnswerText,
                SubmittedAt = answer.SubmittedAt
            };
        }

        public async Task<List<InterviewAnswerResponse>> GetAnswersBySessionAsync(
            long userId,
            long sessionId)
        {
            var session = await _context.InterviewSessions
                .FirstOrDefaultAsync(x =>
                    x.Id == sessionId &&
                    x.UserId == userId);

            if (session == null)
            {
                throw new Exception("Interview session not found.");
            }

            return await _context.InterviewAnswers
                .Include(x => x.InterviewQuestion)
                .Where(x => x.InterviewSessionId == sessionId)
                .Select(x => new InterviewAnswerResponse
                {
                    Id = x.Id,
                    InterviewSessionId = x.InterviewSessionId,
                    InterviewQuestionId = x.InterviewQuestionId,
                    QuestionContent = x.InterviewQuestion.QuestionContent,
                    AnswerText = x.AnswerText,
                    SubmittedAt = x.SubmittedAt
                })
                .ToListAsync();
        }

        public async Task<InterviewAnswerResponse> UpdateAnswerAsync(
            long userId,
            long answerId,
            UpdateInterviewAnswerRequest request)
        {
            var answer = await _context.InterviewAnswers
                .Include(x => x.InterviewSession)
                .Include(x => x.InterviewQuestion)
                .FirstOrDefaultAsync(x =>
                    x.Id == answerId &&
                    x.InterviewSession.UserId == userId);

            if (answer == null)
            {
                throw new Exception("Answer not found.");
            }

            if (answer.InterviewSession.SessionStatus == SessionStatus.COMPLETED)
            {
                throw new Exception("Cannot update answer in completed session.");
            }

            answer.AnswerText = request.AnswerText;
            answer.SubmittedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new InterviewAnswerResponse
            {
                Id = answer.Id,
                InterviewSessionId = answer.InterviewSessionId,
                InterviewQuestionId = answer.InterviewQuestionId,
                QuestionContent = answer.InterviewQuestion.QuestionContent,
                AnswerText = answer.AnswerText,
                SubmittedAt = answer.SubmittedAt
            };
        }
    }
}