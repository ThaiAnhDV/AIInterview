using AIInterviewPlatform.Application.DTOs.Roadmap;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface ILearningRoadmapService
    {
        Task<RoadmapDetailResponse> GenerateRoadmapAsync(
            long userId,
            GenerateRoadmapRequest request);

        Task<List<RoadmapResponse>> GetMyRoadmapsAsync(long userId);

        Task<RoadmapDetailResponse?> GetRoadmapByIdAsync(
            long userId,
            long roadmapId);

        Task<bool> CompleteActivityAsync(
            long userId,
            long activityId);
    }
}