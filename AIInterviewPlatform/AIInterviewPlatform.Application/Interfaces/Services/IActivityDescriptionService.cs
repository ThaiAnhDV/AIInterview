using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IActivityDescriptionService
{
    Task<ActivityDescriptionResponse?> GenerateActivityDescriptionAsync(string skillName);
    
    Task<ActivityDescriptionResponse?> GenerateActivityDescriptionAsync(
        string skillName, 
        string difficultyLevel);
}
