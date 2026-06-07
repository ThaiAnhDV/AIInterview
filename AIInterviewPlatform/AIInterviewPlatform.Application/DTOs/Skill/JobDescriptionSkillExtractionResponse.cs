namespace AIInterviewPlatform.Application.DTOs.Skill;

public class JobDescriptionSkillExtractionResponse
{
    public bool Success { get; set; }
    public bool AiUsed { get; set; }
    public long JobDescriptionId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int ParsedSkillCount { get; set; }
    public int DbSkillCount { get; set; }
    public List<RequiredSkillResponse> Skills { get; set; } = [];
    public string ErrorMessage { get; set; } = string.Empty;
    public SkillExtractionDiagnosticsResponse Diagnostics { get; set; } = new();
}
