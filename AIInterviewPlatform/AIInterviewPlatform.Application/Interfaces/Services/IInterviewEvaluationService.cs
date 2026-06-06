using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IInterviewEvaluationService
{
    Task<EvaluationResultDto> EvaluateAnswerAsync(
        EvaluationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<EvaluationResultDto> EvaluateAnswerAsync(
        string question,
        string answer,
        CancellationToken cancellationToken = default);
}
