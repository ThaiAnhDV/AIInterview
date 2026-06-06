namespace AIInterviewPlatform.Application.DTOs.Skill
{
    public class RequiredSkillResponse
    {
        public long Id { get; set; }

        public long SkillId { get; set; }

        public string SkillName { get; set; } = string.Empty;

        public string? SkillType { get; set; }

        public string? ImportanceLevel { get; set; }
    }
}