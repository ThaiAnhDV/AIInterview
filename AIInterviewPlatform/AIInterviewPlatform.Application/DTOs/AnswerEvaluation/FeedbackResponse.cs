namespace AIInterviewPlatform.Application.DTOs.AnswerEvaluation
{
    public class FeedbackResponse
    {
        public long Id { get; set; }

        public string FeedbackContent { get; set; } = string.Empty;

        public string? FeedbackType { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}