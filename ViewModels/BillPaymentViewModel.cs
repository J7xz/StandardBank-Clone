using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class BillPaymentViewModel
    {
        [Required]
        [Display(Name = "Bill Type")]
        public string BillType { get; set; }

        [Required]
        [Display(Name = "Pay From Account")]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Reference Number")]
        public string ReferenceNumber { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public List<Account> Accounts { get; set; }
        public List<string> BillTypes { get; set; }
    }

    public class BillPaymentConfirmationViewModel
    {
        public string BillType { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; }
        public string PaymentDate { get; set; }
        public string Status { get; set; }
        public decimal NewBalance { get; set; }
    }
}