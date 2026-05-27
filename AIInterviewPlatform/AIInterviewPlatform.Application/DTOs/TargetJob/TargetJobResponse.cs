namespace AIInterviewPlatform.Application.DTOs.TargetJob
{
    public class TargetJobResponse
    {
        public long Id { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string? Industry { get; set; }

        public string? ExperienceLevel { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}