using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class TransactionMonitoringViewModel
    {
        public List<Transaction> Transactions { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public TransactionMonitorFilterViewModel Filter { get; set; }
    }

    public class TransactionMonitorFilterViewModel
    {
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Min Amount")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Max Amount")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; }

        [Display(Name = "Transaction Category")]
        public string TransactionCategory { get; set; }

        [Display(Name = "User")]
        public string UserId { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Search")]
        public string SearchTerm { get; set; }

        public List<string> TransactionTypes { get; set; }
        public List<string> TransactionCategories { get; set; }
        public List<string> Statuses { get; set; }
        public List<User> Users { get; set; }
    }
}