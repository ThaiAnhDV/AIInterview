namespace AIInterviewPlatform.Application.DTOs.TargetJob
{
    public class CreateTargetJobRequest
    {
        public string JobTitle { get; set; } = string.Empty;

        public string? Industry { get; set; }

        public string? ExperienceLevel { get; set; }
    }
}