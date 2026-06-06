namespace AIInterviewPlatform.Application.DTOs.Interview
{
    public class InterviewSessionResponse
    {
        public long Id { get; set; }

        public long TargetJobId { get; set; }

        public string TargetJobTitle { get; set; } = string.Empty;

        public string SessionStatus { get; set; } = string.Empty;

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public List<InterviewQuestionResponse> Questions { get; set; } = new();
    }
}