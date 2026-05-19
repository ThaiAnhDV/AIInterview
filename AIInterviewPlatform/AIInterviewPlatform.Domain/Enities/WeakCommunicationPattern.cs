using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class WeakCommunicationPattern
    {
        public long Id { get; set; }

        public long FeedbackId { get; set; }

        public string PatternName { get; set; } = string.Empty;

        public string? PatternDescription { get; set; }

        public Feedback Feedback { get; set; } = null!;
    }
}
