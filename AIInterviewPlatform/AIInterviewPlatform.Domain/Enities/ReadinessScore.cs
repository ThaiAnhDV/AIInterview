using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class ReadinessScore
    {
        public long Id { get; set; }

        public long? SkillGapAnalysisId { get; set; }

        public long UserId { get; set; }

        public decimal Score { get; set; }

        public ScoreType ScoreType { get; set; } = ScoreType.OVERALL;

        public DateTime CalculatedAt { get; set; } = DateTime.Now;

        public SkillGapAnalysis? SkillGapAnalysis { get; set; }

        public User User { get; set; } = null!;
    }
}
