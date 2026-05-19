using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class InterviewQuestion
    {
        public long Id { get; set; }

        public long InterviewSessionId { get; set; }

        public long CategoryId { get; set; }

        public long? QuestionTemplateId { get; set; }

        public string QuestionContent { get; set; } = string.Empty;

        public QuestionGeneratedBy GeneratedBy { get; set; } = QuestionGeneratedBy.AI;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public InterviewSession InterviewSession { get; set; } = null!;

        public QuestionCategory Category { get; set; } = null!;

        public QuestionTemplate? QuestionTemplate { get; set; }

        public ICollection<InterviewAnswer> InterviewAnswers { get; set; } = new List<InterviewAnswer>();
    }
}
