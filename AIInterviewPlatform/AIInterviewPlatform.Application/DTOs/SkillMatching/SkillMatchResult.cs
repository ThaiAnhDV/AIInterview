namespace AIInterviewPlatform.Application.DTOs.SkillMatching;

public class SkillMatchResult
{
    public List<string> MatchedSkills { get; set; } = [];
    public List<string> MissingSkills { get; set; } = [];
    public List<string> UnmatchedResumeSkills { get; set; } = [];
    public double MatchPercentage { get; set; }
    public double ReadinessScore { get; set; }
}
