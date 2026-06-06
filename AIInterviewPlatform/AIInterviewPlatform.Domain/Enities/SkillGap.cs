using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class SkillGap
    {
        public long Id { get; set; }

        public long SkillGapAnalysisId { get; set; }

        public long SkillId { get; set; }

        public GapLevel? GapLevel { get; set; }

        public string? GapDescription { get; set; }

        public SkillGapAnalysis SkillGapAnalysis { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}
