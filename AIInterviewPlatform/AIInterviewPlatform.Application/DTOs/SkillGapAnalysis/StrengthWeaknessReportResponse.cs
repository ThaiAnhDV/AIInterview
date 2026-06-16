namespace AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;

public class StrengthWeaknessReportResponse
{
    public long Id { get; set; }
    public ReportTypeResponse ReportType { get; set; }
    public string Content { get; set; } = string.Empty;
}

public enum ReportTypeResponse
{
    STRENGTH,
    WEAKNESS
}
