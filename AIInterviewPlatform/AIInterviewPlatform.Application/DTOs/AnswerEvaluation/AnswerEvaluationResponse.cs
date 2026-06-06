namespace AIInterviewPlatform.Application.DTOs.AnswerEvaluation
{
    public class AnswerEvaluationResponse
    {
        public bool Success { get; set; } = true;
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }
        public long Id { get; set; }
        public long InterviewAnswerId { get; set; }
        public decimal ClarityScore { get; set; }
        public decimal StructureScore { get; set; }
        public decimal RelevanceScore { get; set; }
        public decimal OverallScore { get; set; }
        public DateTime EvaluatedAt { get; set; }
        public List<FeedbackResponse> Feedbacks { get; set; } = new();
    }
}
