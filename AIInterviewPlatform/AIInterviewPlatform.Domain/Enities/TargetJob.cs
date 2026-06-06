using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class TargetJob
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string? Industry { get; set; }

        public string? ExperienceLevel { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;

        public ICollection<JobDescription> JobDescriptions { get; set; } = new List<JobDescription>();
        public ICollection<InterviewSession> InterviewSessions { get; set; } = new List<InterviewSession>();
        public ICollection<LearningRoadmap> LearningRoadmaps { get; set; } = new List<LearningRoadmap>();
    }
}
