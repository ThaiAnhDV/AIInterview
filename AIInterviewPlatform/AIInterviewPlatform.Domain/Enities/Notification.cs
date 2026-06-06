using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class Notification
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public NotificationType? NotificationType { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;
    }
}
