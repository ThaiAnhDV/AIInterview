using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Application.DTOs.Profile
{
    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? EducationLevel { get; set; }

        public string? CareerGoal { get; set; }
    }
}
