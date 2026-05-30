namespace AIInterviewPlatform.Application.DTOs.Skill
{
    public class CreateSkillRequest
    {
        public string SkillName { get; set; } = string.Empty;

        public string? SkillType { get; set; }
    }
}