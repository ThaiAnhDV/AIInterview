using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class JobDescription
    {
        public long Id { get; set; }

        public long TargetJobId { get; set; }

        public string Content { get; set; } = string.Empty;

        public JobDescriptionSourceType SourceType { get; set; } = JobDescriptionSourceType.MANUAL;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public TargetJob TargetJob { get; set; } = null!;

        public ICollection<RequiredSkill> RequiredSkills { get; set; } = new List<RequiredSkill>();
        public ICollection<SkillGapAnalysis> SkillGapAnalyses { get; set; } = new List<SkillGapAnalysis>();
    }
}
