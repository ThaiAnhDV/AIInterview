using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class AnswerEvaluation
    {
        public long Id { get; set; }

        public long InterviewAnswerId { get; set; }

        public decimal? ClarityScore { get; set; }

        public decimal? StructureScore { get; set; }

        public decimal? RelevanceScore { get; set; }

        public decimal? OverallScore { get; set; }

        public DateTime EvaluatedAt { get; set; } = DateTime.Now;

        public InterviewAnswer InterviewAnswer { get; set; } = null!;
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
