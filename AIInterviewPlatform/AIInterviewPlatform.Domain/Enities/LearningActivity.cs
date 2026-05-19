using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class LearningActivity
    {
        public long Id { get; set; }

        public long RoadmapMilestoneId { get; set; }

        public long? SkillId { get; set; }

        public string ActivityTitle { get; set; } = string.Empty;

        public string? ActivityDescription { get; set; }

        public ActivityType? ActivityType { get; set; }

        public bool IsCompleted { get; set; } = false;

        public RoadmapMilestone RoadmapMilestone { get; set; } = null!;

        public Skill? Skill { get; set; }
        public ICollection<PracticeHistory> PracticeHistories { get; set; } = new List<PracticeHistory>();
    }
}
