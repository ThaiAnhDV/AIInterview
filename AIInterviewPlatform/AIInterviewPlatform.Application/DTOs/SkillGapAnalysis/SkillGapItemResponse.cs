namespace AIInterviewPlatform.Application.DTOs.SkillGapAnalysis
{
    public class SkillGapItemResponse
    {
        public long SkillId { get; set; }

        public string SkillName { get; set; } = string.Empty;

        public string GapLevel { get; set; } = string.Empty;

        public string? GapDescription { get; set; }
    }
}