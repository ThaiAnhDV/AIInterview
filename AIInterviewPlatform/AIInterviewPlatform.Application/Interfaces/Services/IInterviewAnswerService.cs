using AIInterviewPlatform.Application.DTOs.InterviewAnswer;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface IInterviewAnswerService
    {
        Task<InterviewAnswerResponse> SubmitAnswerAsync(
            long userId,
            long sessionId,
            SubmitInterviewAnswerRequest request);

        Task<List<InterviewAnswerResponse>> GetAnswersBySessionAsync(
            long userId,
            long sessionId);

        Task<InterviewAnswerResponse> UpdateAnswerAsync(
            long userId,
            long answerId,
            UpdateInterviewAnswerRequest request);
    }
}