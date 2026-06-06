using AIInterviewPlatform.Application.DTOs.JobDescriptionSkillExtraction;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IJobDescriptionSkillExtractionService
{
    Task<JobDescriptionSkillExtractionResult> ExtractRequiredSkillsAsync(string jobDescriptionContent);
}
