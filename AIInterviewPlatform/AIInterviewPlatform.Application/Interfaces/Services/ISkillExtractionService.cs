using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.SkillExtraction;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface ISkillExtractionService
{
    Task<AIServiceResult<SkillExtractionResult>> ExtractSkillsFromResumeAsync(string resumeContent);
}
