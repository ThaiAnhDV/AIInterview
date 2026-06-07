using AIInterviewPlatform.Application.DTOs.Skill;
using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.JobDescriptionSkillExtraction;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface ISkillService
    {
        Task<List<SkillResponse>> GetAllSkillsAsync();

        Task<SkillResponse> CreateSkillAsync(CreateSkillRequest request);

        Task<List<RequiredSkillResponse>>
            GetRequiredSkillsByJobDescriptionAsync(long jobDescriptionId);

        Task<JobDescriptionSkillExtractionResponse> ExtractSkillsFromJobDescriptionAsync(
            long jobDescriptionId,
            string? languageCode = null);
    }
}