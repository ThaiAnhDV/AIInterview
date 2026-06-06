using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIInterviewPlatform.Domain.Enities
{
    public class SystemLog
    {
        public long Id { get; set; }

        public long? UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User? User { get; set; }
    }
}
