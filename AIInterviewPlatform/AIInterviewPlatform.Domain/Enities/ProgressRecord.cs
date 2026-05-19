using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class ProgressRecord
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public decimal OverallProgress { get; set; } = 0;

        public DateTime RecordedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;
    }
}
