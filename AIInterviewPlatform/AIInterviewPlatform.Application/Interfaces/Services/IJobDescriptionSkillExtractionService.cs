using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.JobDescriptionSkillExtraction;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IJobDescriptionSkillExtractionService
{
    Task<AIServiceResult<JobDescriptionSkillExtractionResult>> ExtractRequiredSkillsAsync(string jobDescriptionContent);
}
