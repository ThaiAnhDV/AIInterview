using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IActivityDescriptionService
{
    Task<ActivityDescriptionResponse?> GenerateActivityDescriptionAsync(
        string skillName,
        string? languageCode = null);
    
    Task<ActivityDescriptionResponse?> GenerateActivityDescriptionAsync(
        string skillName, 
        string difficultyLevel,
        string? languageCode = null);
}
