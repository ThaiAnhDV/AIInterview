using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class RoadmapProgress
    {
        public long Id { get; set; }

        public long LearningRoadmapId { get; set; }

        public decimal CompletionPercentage { get; set; } = 0;

        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

        public LearningRoadmap LearningRoadmap { get; set; } = null!;
    }
}
