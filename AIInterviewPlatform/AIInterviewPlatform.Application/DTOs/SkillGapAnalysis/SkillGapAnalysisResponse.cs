namespace AIInterviewPlatform.Application.DTOs.SkillGapAnalysis
{
    public class SkillGapAnalysisResponse
    {
        public long Id { get; set; }

        public long ResumeId { get; set; }

        public long JobDescriptionId { get; set; }

        public decimal ReadinessScore { get; set; }

        public List<string> MatchedSkills { get; set; } = new();

        public List<SkillGapItemResponse> MissingSkills { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}