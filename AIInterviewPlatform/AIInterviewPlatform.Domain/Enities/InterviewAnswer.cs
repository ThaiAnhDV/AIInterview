using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class InterviewAnswer
    {
        public long Id { get; set; }

        public long InterviewSessionId { get; set; }

        public long InterviewQuestionId { get; set; }

        public string AnswerText { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public InterviewSession InterviewSession { get; set; } = null!;

        public InterviewQuestion InterviewQuestion { get; set; } = null!;

        public AnswerEvaluation? AnswerEvaluation { get; set; }
    }
}
