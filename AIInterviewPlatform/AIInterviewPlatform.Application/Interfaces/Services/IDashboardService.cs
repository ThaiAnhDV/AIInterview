using AIInterviewPlatform.Application.DTOs.Dashboard;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<ReadinessDashboardResponse> GetReadinessDashboardAsync(long userId);
    Task<SkillGapsDashboardResponse?> GetSkillGapsDashboardAsync(long userId);
    Task<HistoryDashboardResponse> GetHistoryDashboardAsync(long userId);
    Task<DashboardDto> GetDashboardAsync(long userId);
}
