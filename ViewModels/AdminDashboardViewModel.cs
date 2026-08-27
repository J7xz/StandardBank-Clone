using System.Collections.Generic;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalAccounts { get; set; }
        public decimal TotalBalance { get; set; }
        public int TotalTransactions { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }

        public List<Transaction> RecentTransactions { get; set; }
        public List<User> RecentUsers { get; set; }
        public List<AdminLog> RecentLogs { get; set; }

        public decimal TodayTransactions { get; set; }
        public int TodayRegistrations { get; set; }
    }
}