using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class RoadmapMilestone
    {
        public long Id { get; set; }

        public long LearningRoadmapId { get; set; }

        public string MilestoneTitle { get; set; } = string.Empty;

        public int MilestoneOrder { get; set; }

        public bool IsCompleted { get; set; } = false;

        public int EstimatedDays { get; set; } = 7;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public LearningRoadmap LearningRoadmap { get; set; } = null!;

        public ICollection<LearningActivity> LearningActivities { get; set; } = new List<LearningActivity>();
    }
}
