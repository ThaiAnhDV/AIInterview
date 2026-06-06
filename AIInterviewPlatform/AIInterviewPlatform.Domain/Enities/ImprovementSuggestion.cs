using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class ImprovementSuggestion
    {
        public long Id { get; set; }

        public long FeedbackId { get; set; }

        public string SuggestionContent { get; set; } = string.Empty;

        public PriorityLevel? PriorityLevel { get; set; }

        public Feedback Feedback { get; set; } = null!;
    }
}
