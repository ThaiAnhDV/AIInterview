using AIInterviewPlatform.Application.DTOs.SkillExtraction;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface ISkillExtractionService
{
    Task<SkillExtractionResult> ExtractSkillsFromResumeAsync(string resumeContent);
}
