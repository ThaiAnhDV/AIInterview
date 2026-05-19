using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class StrengthWeaknessReport
    {
        public long Id { get; set; }

        public long SkillGapAnalysisId { get; set; }

        public ReportType ReportType { get; set; }

        public string Content { get; set; } = string.Empty;

        public SkillGapAnalysis SkillGapAnalysis { get; set; } = null!;
    }
}
