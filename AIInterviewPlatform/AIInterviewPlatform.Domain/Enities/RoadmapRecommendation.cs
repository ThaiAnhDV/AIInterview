using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class RoadmapRecommendation
    {
        public long Id { get; set; }

        public long LearningRoadmapId { get; set; }

        public long RecommendationId { get; set; }

        public LearningRoadmap LearningRoadmap { get; set; } = null!;

        public Recommendation Recommendation { get; set; } = null!;
    }
}
