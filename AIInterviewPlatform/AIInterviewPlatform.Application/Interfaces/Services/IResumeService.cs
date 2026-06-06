using AIInterviewPlatform.Application.DTOs.Resume;
using Microsoft.AspNetCore.Http;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface IResumeService
    {
        Task<ResumeResponse> UploadResumeAsync(long userId, IFormFile file);

        Task<List<ResumeResponse>> GetMyResumesAsync(long userId);

        Task<bool> SetActiveResumeAsync(long userId, long resumeId);

        Task<bool> DeleteResumeAsync(long userId, long resumeId);
    }
}