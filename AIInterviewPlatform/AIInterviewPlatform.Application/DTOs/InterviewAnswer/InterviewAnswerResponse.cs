namespace AIInterviewPlatform.Application.DTOs.InterviewAnswer
{
    public class InterviewAnswerResponse
    {
        public long Id { get; set; }

        public long InterviewSessionId { get; set; }

        public long InterviewQuestionId { get; set; }

        public string QuestionContent { get; set; } = string.Empty;

        public string AnswerText { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }
    }
}