using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class Feedback
    {
        public long Id { get; set; }

        public long AnswerEvaluationId { get; set; }

        public string FeedbackContent { get; set; } = string.Empty;

        public FeedbackType? FeedbackType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public AnswerEvaluation AnswerEvaluation { get; set; } = null!;

        public ICollection<ImprovementSuggestion> ImprovementSuggestions { get; set; } = new List<ImprovementSuggestion>();

        public ICollection<WeakCommunicationPattern> WeakCommunicationPatterns { get; set; } = new List<WeakCommunicationPattern>();

        public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    }
}
