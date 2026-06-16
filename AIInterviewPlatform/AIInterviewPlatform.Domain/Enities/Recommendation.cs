using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class Recommendation
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long SkillGapAnalysisId { get; set; }

        public long SkillId { get; set; }

        public long? FeedbackId { get; set; }

        public string RecommendationTitle { get; set; } = string.Empty;

        public string RecommendationContent { get; set; } = string.Empty;

        public RecommendationType? RecommendationType { get; set; }

        public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.MEDIUM;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;

        public SkillGapAnalysis SkillGapAnalysis { get; set; } = null!;

        public Skill Skill { get; set; } = null!;

        public Feedback? Feedback { get; set; }

        public ICollection<RoadmapRecommendation> RoadmapRecommendations { get; set; } = new List<RoadmapRecommendation>();
    }
}
