using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IInterviewEvaluationApplicationService
{
    Task<EvaluationResultDto> EvaluateAnswerAsync(
        long answerId,
        CancellationToken cancellationToken = default);

    Task<EvaluationResultDto> EvaluateAnswerAsync(
        long userId,
        long answerId,
        CancellationToken cancellationToken = default);
}
