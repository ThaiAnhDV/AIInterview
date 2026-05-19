using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class UsageStatistic
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public int TotalSessions { get; set; } = 0;

        public int TotalQuestionsAnswered { get; set; } = 0;

        public decimal AverageScore { get; set; } = 0;

        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;
    }
}
