namespace AIInterviewPlatform.Application.DTOs.Interview.Requests;

public class StartMockInterviewRequest
{
    public required long TargetJobId { get; init; }
    public required long SkillGapAnalysisId { get; init; }
    public string? LanguageCode { get; init; }
}
