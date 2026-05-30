namespace AIInterviewPlatform.Application.DTOs.Interview
{
    public class InterviewQuestionResponse
    {
        public long Id { get; set; }

        public string QuestionContent { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string GeneratedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}