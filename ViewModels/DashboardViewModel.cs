using System.Collections.Generic;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class DashboardViewModel
    {
        public string FullName { get; set; }
        public decimal TotalBalance { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
        public List<SavingsGoal> SavingsGoals { get; set; }
        public List<Budget> Budgets { get; set; }
        public int TotalAccounts { get; set; }
        public int RecentTransactionsCount { get; set; }
    }
}