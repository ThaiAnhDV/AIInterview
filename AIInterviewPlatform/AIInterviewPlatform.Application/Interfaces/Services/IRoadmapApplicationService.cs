using AIInterviewPlatform.Application.DTOs.Roadmap.Requests;
using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IRoadmapApplicationService
{
    Task<RoadmapDto> GenerateRoadmapFromMissingSkillsAsync(
        long userId,
        GenerateRoadmapFromMissingSkillsRequest request);

    Task<RoadmapDto> GenerateRoadmapFromAnalysisAsync(
        long userId,
        GenerateRoadmapFromAnalysisRequest request);

    Task<RoadmapDto?> GetRoadmapByIdAsync(long userId, long roadmapId);

    Task<List<RoadmapSummaryDto>> GetUserRoadmapsAsync(long userId);

    Task<ActivityCompletionResultDto> CompleteActivityAsync(
        long userId,
        long activityId);
}
