using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class InterviewSession
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long TargetJobId { get; set; }

        public SessionStatus SessionStatus { get; set; } = SessionStatus.IN_PROGRESS;

        public DateTime StartedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }

        public User User { get; set; } = null!;

        public TargetJob TargetJob { get; set; } = null!;

        public ICollection<InterviewQuestion> InterviewQuestions { get; set; } = new List<InterviewQuestion>();

        public ICollection<InterviewAnswer> InterviewAnswers { get; set; } = new List<InterviewAnswer>();
        public ICollection<PracticeHistory> PracticeHistories { get; set; } = new List<PracticeHistory>();
    }
}
