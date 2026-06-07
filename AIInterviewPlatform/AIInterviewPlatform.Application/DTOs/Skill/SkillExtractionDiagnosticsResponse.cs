namespace AIInterviewPlatform.Application.DTOs.Skill;

public class SkillExtractionDiagnosticsResponse
{
    public bool Success { get; set; }
    public bool AiUsed { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int? HttpStatus { get; set; }
    public string RawResponse { get; set; } = string.Empty;
}
