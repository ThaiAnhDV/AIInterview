namespace AIInterviewPlatform.Application.DTOs.SkillGapAnalysis
{
public class CreateSkillGapAnalysisRequest
{
    public long ResumeId { get; set; }

    public long JobDescriptionId { get; set; }

    public string? LanguageCode { get; set; }
}
}