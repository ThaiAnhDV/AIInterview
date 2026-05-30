namespace AIInterviewPlatform.Application.DTOs.Roadmap
{
    public class RoadmapResponse
    {
        public long Id { get; set; }

        public string RoadmapTitle { get; set; } = string.Empty;

        public string RoadmapStatus { get; set; } = string.Empty;

        public decimal CompletionPercentage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}