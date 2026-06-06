using AIInterviewPlatform.Application.DTOs.AnswerEvaluation;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface IAnswerEvaluationService
    {
        Task<AnswerEvaluationResponse> EvaluateAnswerAsync(long answerId);

        Task<AnswerEvaluationResponse?> GetEvaluationAsync(long answerId);

        Task<SessionFeedbackResponse> GetSessionFeedbackAsync(long sessionId);
    }
}
