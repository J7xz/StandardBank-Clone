using System;
using System.ComponentModel.DataAnnotations;

namespace StandardBank.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public string NotificationType { get; set; } // Email, InApp

        public DateTime DateSent { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public DateTime? DateRead { get; set; }

        public string LinkUrl { get; set; }

        // Foreign Key
        [Required]
        public string UserId { get; set; }

        // Navigation property
        public virtual User User { get; set; }
    }
}