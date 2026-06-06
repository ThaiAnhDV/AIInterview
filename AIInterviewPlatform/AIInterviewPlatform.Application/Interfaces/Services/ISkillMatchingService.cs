using AIInterviewPlatform.Application.DTOs.SkillMatching;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface ISkillMatchingService
{
    SkillMatchResult MatchSkills(SkillMatchRequest request);
}
