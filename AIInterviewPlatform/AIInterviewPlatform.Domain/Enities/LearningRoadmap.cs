using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class LearningRoadmap
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long? TargetJobId { get; set; }

        public long? SkillGapAnalysisId { get; set; }

        public string RoadmapTitle { get; set; } = string.Empty;

        public RoadmapStatus RoadmapStatus { get; set; } = RoadmapStatus.ACTIVE;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;

        public TargetJob? TargetJob { get; set; }

        public SkillGapAnalysis? SkillGapAnalysis { get; set; }

        public ICollection<RoadmapMilestone> RoadmapMilestones { get; set; } = new List<RoadmapMilestone>();

        public RoadmapProgress? RoadmapProgress { get; set; }

        public ICollection<RoadmapRecommendation> RoadmapRecommendations { get; set; } = new List<RoadmapRecommendation>();
    }
}
