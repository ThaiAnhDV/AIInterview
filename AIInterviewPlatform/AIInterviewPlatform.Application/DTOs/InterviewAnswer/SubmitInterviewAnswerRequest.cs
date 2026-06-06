namespace AIInterviewPlatform.Application.DTOs.InterviewAnswer
{
    public class SubmitInterviewAnswerRequest
    {
        public long InterviewSessionId { get; set; }

        public long InterviewQuestionId { get; set; }

        public string AnswerText { get; set; } = string.Empty;
    }
}