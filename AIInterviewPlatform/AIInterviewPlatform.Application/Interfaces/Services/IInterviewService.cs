using AIInterviewPlatform.Application.DTOs.Interview;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface IInterviewService
    {
        Task<InterviewSessionResponse> StartInterviewAsync(
            long userId,
            StartInterviewRequest request);

        Task<List<InterviewSessionResponse>> GetMySessionsAsync(
            long userId);

        Task<InterviewSessionResponse?> GetByIdAsync(
            long userId,
            long sessionId);

        Task<bool> CompleteSessionAsync(
            long userId,
            long sessionId);
    }
}