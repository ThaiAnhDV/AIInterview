namespace AIInterviewPlatform.Application.DTOs.InterviewAnswer
{
    public class AnswerEvaluationResponse
    {
        public long Id { get; set; }

        public long InterviewAnswerId { get; set; }

        public decimal ClarityScore { get; set; }

        public decimal StructureScore { get; set; }

        public decimal RelevanceScore { get; set; }

        public decimal OverallScore { get; set; }

        public List<string> Feedbacks { get; set; } = new();

        public DateTime EvaluatedAt { get; set; }
    }
}