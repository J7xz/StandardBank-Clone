using System;
using System.ComponentModel.DataAnnotations;

namespace StandardBank.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string TransactionType { get; set; } // Debit, Credit

        [Required]
        public string TransactionCategory { get; set; } // Transfer, BillPayment, Deposit, Withdrawal

        public string Description { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Completed"; // Pending, Completed, Failed

        public decimal BalanceAfterTransaction { get; set; }

        public string ReferenceNumber { get; set; }

        // Foreign Key
        [Required]
        public int AccountId { get; set; }

        // Navigation property
        public virtual Account Account { get; set; }
    }
}