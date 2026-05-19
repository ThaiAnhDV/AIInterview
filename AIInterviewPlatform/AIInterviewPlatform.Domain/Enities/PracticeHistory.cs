using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class PracticeHistory
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long? InterviewSessionId { get; set; }

        public long? LearningActivityId { get; set; }

        public ActivityType? ActivityType { get; set; }

        public DateTime PracticedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;

        public InterviewSession? InterviewSession { get; set; }

        public LearningActivity? LearningActivity { get; set; }
    }
}
