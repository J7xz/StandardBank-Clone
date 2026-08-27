using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class TransactionHistoryViewModel
    {
        public List<Transaction> Transactions { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public int TotalTransactions { get; set; }

        // Filters
        public string FilterType { get; set; } // All, Debit, Credit
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchTerm { get; set; }
        public int? AccountId { get; set; }

        public List<Account> Accounts { get; set; }
        public List<string> TransactionTypes { get; set; }
    }

    // Rename this to TransactionHistoryFilterViewModel to avoid conflict
    public class TransactionHistoryFilterViewModel
    {
        [Display(Name = "Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; }

        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Search")]
        public string SearchTerm { get; set; }

        public List<Account> Accounts { get; set; }
        public List<string> TransactionTypes { get; set; }
    }
}