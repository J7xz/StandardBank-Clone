using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class UserManagementViewModel
    {
        public List<UserViewModel> Users { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }
        public string SearchTerm { get; set; }
        public string FilterStatus { get; set; } // All, Active, Locked
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IDNumber { get; set; }
        public string DateRegistered { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public int TotalAccounts { get; set; }
        public decimal TotalBalance { get; set; }
        public string LastLoginDate { get; set; }
    }

    public class UserDetailsViewModel
    {
        public User User { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
        public decimal TotalBalance { get; set; }
        public int TotalTransactions { get; set; }
    }
}