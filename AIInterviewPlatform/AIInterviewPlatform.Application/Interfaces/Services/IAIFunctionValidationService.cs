using AIInterviewPlatform.Application.DTOs.AI;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IAIFunctionValidationService
{
    Task<AIFunctionValidationResponse> ValidateAsync(CancellationToken cancellationToken = default);
}
