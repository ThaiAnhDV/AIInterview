namespace AIInterviewPlatform.Application.DTOs.SkillMatching;

public class SkillMatchRequest
{
    public List<string> ResumeSkills { get; set; } = [];
    public List<string> RequiredSkills { get; set; } = [];
}
