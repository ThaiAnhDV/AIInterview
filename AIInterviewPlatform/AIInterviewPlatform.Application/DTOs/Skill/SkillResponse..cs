namespace AIInterviewPlatform.Application.DTOs.Skill
{
    public class SkillResponse
    {
        public long Id { get; set; }

        public string SkillName { get; set; } = string.Empty;

        public string? SkillType { get; set; }
    }
}