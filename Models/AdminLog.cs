using System;
using System.ComponentModel.DataAnnotations;

namespace StandardBank.Models
{
    public class AdminLog
    {
        public int Id { get; set; }

        [Required]
        public string Action { get; set; }

        public string UserId { get; set; } // Admin who performed action

        public string TargetUserId { get; set; } // User affected by action

        public string Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string IPAddress { get; set; }

        public string UserAgent { get; set; }
    }
}