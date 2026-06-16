using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class MatchedSkill
    {
        public long Id { get; set; }

        public long SkillGapAnalysisId { get; set; }

        public long SkillId { get; set; }

        public double MatchScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public SkillGapAnalysis SkillGapAnalysis { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}
