using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class RequiredSkill
    {
        public long Id { get; set; }

        public long JobDescriptionId { get; set; }

        public long SkillId { get; set; }

        public ImportanceLevel? ImportanceLevel { get; set; }

        public JobDescription JobDescription { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}
