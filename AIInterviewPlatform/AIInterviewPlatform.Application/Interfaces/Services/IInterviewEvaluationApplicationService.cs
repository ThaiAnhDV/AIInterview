using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IInterviewEvaluationApplicationService
{
    Task<EvaluationResultDto> EvaluateAnswerAsync(
        long answerId,
        string? languageCode = null,
        CancellationToken cancellationToken = default);

    Task<EvaluationResultDto> EvaluateAnswerAsync(
        long userId,
        long answerId,
        string? languageCode = null,
        CancellationToken cancellationToken = default);
}
