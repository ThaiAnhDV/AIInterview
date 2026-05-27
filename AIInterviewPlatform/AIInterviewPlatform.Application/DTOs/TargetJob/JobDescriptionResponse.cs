namespace AIInterviewPlatform.Application.DTOs.TargetJob
{
    public class JobDescriptionResponse
    {
        public long Id { get; set; }

        public long TargetJobId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}