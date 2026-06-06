using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IMilestoneGeneratorService
{
    List<MilestoneDto> GenerateMilestones(List<string> missingSkills);
    
    List<MilestoneDto> GenerateMilestones(List<SkillGapForRoadmapDto> skillGaps);
}

public interface IMilestoneGenerator
{
    MilestoneDto GenerateSkillMilestone(string skillName, int order);
    
    MilestoneDto GenerateMockInterviewMilestone(int order);
}
