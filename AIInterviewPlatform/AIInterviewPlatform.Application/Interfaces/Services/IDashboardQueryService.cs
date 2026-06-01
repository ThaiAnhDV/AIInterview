using AIInterviewPlatform.Application.DTOs.Dashboard;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IDashboardQueryService
{
    Task<DashboardDto> GetDashboardAsync(long userId, CancellationToken cancellationToken = default);
}
