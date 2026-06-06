using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class QuestionCategory
    {
        public long Id { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public ICollection<QuestionTemplate> QuestionTemplates { get; set; } = new List<QuestionTemplate>();

        public ICollection<InterviewQuestion> InterviewQuestions { get; set; } = new List<InterviewQuestion>();
    }
}
