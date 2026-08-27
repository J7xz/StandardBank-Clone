using System.Collections.Generic;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class AccountViewModel
    {
        public List<Account> Accounts { get; set; }
        public decimal TotalBalance { get; set; }
        public int ActiveAccounts { get; set; }
    }

    public class CreateAccountViewModel
    {
        public string AccountType { get; set; } // Cheque, Savings, Student
        public string AccountName { get; set; }
        public decimal InitialDeposit { get; set; }
        public List<string> AccountTypes { get; set; }
    }
}