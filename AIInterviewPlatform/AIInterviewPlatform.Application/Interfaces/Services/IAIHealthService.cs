using AIInterviewPlatform.Application.DTOs.AI;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IAIHealthService
{
    Task<AIHealthResponse> CheckHealthAsync();
}
