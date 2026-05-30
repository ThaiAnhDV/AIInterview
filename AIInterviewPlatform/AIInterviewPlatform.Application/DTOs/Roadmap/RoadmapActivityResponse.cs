namespace AIInterviewPlatform.Application.DTOs.Roadmap
{
    public class RoadmapActivityResponse
    {
        public long Id { get; set; }

        public string ActivityTitle { get; set; } = string.Empty;

        public string? ActivityDescription { get; set; }

        public string? ActivityType { get; set; }

        public bool IsCompleted { get; set; }
    }
}