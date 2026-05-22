using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Application.DTOs.Profile
{
    public class ProfileResponseDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? EducationLevel { get; set; }
        public string? CareerGoal { get; set; }

        public string UserType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
