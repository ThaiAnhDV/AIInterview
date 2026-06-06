using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class QuestionTemplate
    {
        public long Id { get; set; }

        public long CategoryId { get; set; }

        public string TemplateContent { get; set; } = string.Empty;

        public DifficultyLevel? DifficultyLevel { get; set; }

        public bool IsActive { get; set; } = true;

        public long? CreatedByAdminId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public QuestionCategory Category { get; set; } = null!;

        public User? CreatedByAdmin { get; set; }

        public ICollection<InterviewQuestion> InterviewQuestions { get; set; } = new List<InterviewQuestion>();
    }
}
