namespace AIInterviewPlatform.Application.DTOs.TargetJob
{
    public class UpdateTargetJobRequest
    {
        public string JobTitle { get; set; } = string.Empty;

        public string? Industry { get; set; }

        public string? ExperienceLevel { get; set; }
    }
}