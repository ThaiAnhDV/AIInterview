using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.JobDescriptionSkillExtraction;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IJobDescriptionSkillExtractionService
{
    string ModelName { get; }
    string Endpoint { get; }

    Task<AIServiceResult<JobDescriptionSkillExtractionResult>> ExtractRequiredSkillsAsync(
        string jobDescriptionContent,
        string? languageCode = null);
}
