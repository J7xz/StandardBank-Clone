using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StandardBank.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; }

        [Required]
        public string AccountType { get; set; } // Cheque, Savings, Student

        [Required]
        public decimal Balance { get; set; } = 0;

        public DateTime DateOpened { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public string AccountName { get; set; }

        // Foreign Key
        [Required]
        public string UserId { get; set; }
        public string AccountNumberWithBalance
        {
            get
            {
                return $"{AccountNumber} - {Balance.ToString("C")}";
            }
        }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}