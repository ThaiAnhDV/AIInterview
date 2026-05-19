using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class Skill
    {
        public long Id { get; set; }

        public string SkillName { get; set; } = string.Empty;

        public string? SkillType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<RequiredSkill> RequiredSkills { get; set; } = new List<RequiredSkill>();
        public ICollection<SkillGap> SkillGaps { get; set; } = new List<SkillGap>();
        public ICollection<LearningActivity> LearningActivities { get; set; } = new List<LearningActivity>();
        public ICollection<SkillImprovementTrend> SkillImprovementTrends { get; set; } = new List<SkillImprovementTrend>();
    }
}
