using AIInterviewPlatform.Domain.Enities;

namespace AIInterviewPlatform.Application.Interfaces.Repositories;

public interface IInterviewSessionRepository
{
    Task<InterviewSession> CreateSessionWithQuestionsAsync(
        long userId,
        long targetJobId,
        long skillGapAnalysisId,
        List<(string QuestionContent, string Category, string SkillArea)> questions,
        bool isFallback,
        CancellationToken cancellationToken = default);

    Task<InterviewSession?> GetSessionWithQuestionsAsync(
        long sessionId,
        CancellationToken cancellationToken = default);

    Task<InterviewSession?> GetSessionByIdAsync(
        long sessionId,
        CancellationToken cancellationToken = default);

    Task<List<InterviewSession>> GetUserSessionsAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task UpdateSessionStatusAsync(
        long sessionId,
        Domain.Enum.SessionStatus status,
        CancellationToken cancellationToken = default);
}
