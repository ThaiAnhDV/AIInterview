using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class SkillGapAnalysis
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long ResumeId { get; set; }

        public long JobDescriptionId { get; set; }

        public AnalysisStatus AnalysisStatus { get; set; } = AnalysisStatus.COMPLETED;

        public string? LanguageCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;

        public Resume Resume { get; set; } = null!;

        public JobDescription JobDescription { get; set; } = null!;

        public ICollection<SkillGap> SkillGaps { get; set; } = new List<SkillGap>();

        public ICollection<MatchedSkill> MatchedSkills { get; set; } = new List<MatchedSkill>();

        public ICollection<ReadinessScore> ReadinessScores { get; set; } = new List<ReadinessScore>();

        public ICollection<StrengthWeaknessReport> StrengthWeaknessReports { get; set; } = new List<StrengthWeaknessReport>();
        public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
        public ICollection<LearningRoadmap> LearningRoadmaps { get; set; } = new List<LearningRoadmap>();
    }
}
