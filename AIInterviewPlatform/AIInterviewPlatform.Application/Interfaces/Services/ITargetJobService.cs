using AIInterviewPlatform.Application.DTOs.TargetJob;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface ITargetJobService
    {
        Task<TargetJobResponse> CreateTargetJobAsync(
            long userId,
            CreateTargetJobRequest request
        );

        Task<List<TargetJobResponse>> GetMyTargetJobsAsync(long userId);

        Task<TargetJobResponse> GetTargetJobByIdAsync(
            long userId,
            long targetJobId
        );

        Task<TargetJobResponse> UpdateTargetJobAsync(
            long userId,
            long targetJobId,
            UpdateTargetJobRequest request
        );

        Task<bool> DeleteTargetJobAsync(
            long userId,
            long targetJobId
        );

        Task<JobDescriptionResponse> AddJobDescriptionAsync(
            long userId,
            long targetJobId,
            CreateJobDescriptionRequest request
        );

        Task<JobDescriptionResponse?> GetJobDescriptionAsync(
            long userId,
            long targetJobId
        );
    }
}