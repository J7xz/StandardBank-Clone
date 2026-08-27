using System.Collections.Generic;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class NotificationViewModel
    {
        public List<Notification> Notifications { get; set; }
        public int UnreadCount { get; set; }
        public int TotalNotifications { get; set; }
    }
}