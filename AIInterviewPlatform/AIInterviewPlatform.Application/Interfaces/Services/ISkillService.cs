using AIInterviewPlatform.Application.DTOs.Skill;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface ISkillService
    {
        Task<List<SkillResponse>> GetAllSkillsAsync();

        Task<SkillResponse> CreateSkillAsync(CreateSkillRequest request);

        Task<List<RequiredSkillResponse>>
            GetRequiredSkillsByJobDescriptionAsync(long jobDescriptionId);

        Task ExtractSkillsFromJobDescriptionAsync(long jobDescriptionId);
    }
}