using AIInterviewPlatform.Application.DTOs.Interview.Requests;
using AIInterviewPlatform.Application.DTOs.Interview.Responses;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IMockInterviewApplicationService
{
    Task<InterviewQuestionGenerationResult> StartMockInterviewAsync(
        long userId,
        StartMockInterviewRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateUserOwnsResourcesAsync(
        long userId,
        long targetJobId,
        long skillGapAnalysisId,
        CancellationToken cancellationToken = default);
}
