using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class Resume
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public string? ParsedContent { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;
        public ICollection<SkillGapAnalysis> SkillGapAnalyses { get; set; } = new List<SkillGapAnalysis>();
    }
}
