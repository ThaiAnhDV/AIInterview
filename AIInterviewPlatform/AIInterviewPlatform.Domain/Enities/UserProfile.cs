using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class UserProfile
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public string? EducationLevel { get; set; }

        public string? CareerGoal { get; set; }

        public string? PreferredLanguageCode { get; set; }

        public string? LanguageCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
