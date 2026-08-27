using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class DepositViewModel
    {
        [Required]
        [Display(Name = "Deposit To Account")]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Reference")]
        public string Reference { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public List<Account> Accounts { get; set; }
    }

    public class DepositConfirmationViewModel
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string DepositDate { get; set; }
        public decimal NewBalance { get; set; }
        public string TransactionReference { get; set; }
    }
}