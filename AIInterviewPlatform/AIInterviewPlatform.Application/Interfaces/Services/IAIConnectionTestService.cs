using AIInterviewPlatform.Application.DTOs.AI;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IAIConnectionTestService
{
    Task<AIConnectionTestResponse> PingAsync(CancellationToken cancellationToken = default);
}
