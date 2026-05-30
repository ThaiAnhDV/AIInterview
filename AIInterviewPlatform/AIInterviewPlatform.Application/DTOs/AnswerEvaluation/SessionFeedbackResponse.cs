namespace AIInterviewPlatform.Application.DTOs.AnswerEvaluation
{
    public class SessionFeedbackResponse
    {
        public long InterviewSessionId { get; set; }

        public decimal AverageScore { get; set; }

        public List<AnswerEvaluationResponse> Evaluations { get; set; } = new();
    }
}