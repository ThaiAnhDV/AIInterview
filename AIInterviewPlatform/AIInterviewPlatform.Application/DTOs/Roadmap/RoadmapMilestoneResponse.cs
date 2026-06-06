namespace AIInterviewPlatform.Application.DTOs.Roadmap
{
    public class RoadmapMilestoneResponse
    {
        public long Id { get; set; }

        public string MilestoneTitle { get; set; } = string.Empty;

        public int MilestoneOrder { get; set; }

        public bool IsCompleted { get; set; }

        public List<RoadmapActivityResponse> Activities { get; set; } = new();
    }
}