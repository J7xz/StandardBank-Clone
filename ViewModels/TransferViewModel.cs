using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class TransferViewModel
    {
        // ===== INTERNAL TRANSFER =====
        [Required]
        [Display(Name = "From Account")]
        public int FromAccountId { get; set; }

        [Required]
        [Display(Name = "To Account")]
        public int ToAccountId { get; set; }

        // ===== AMOUNT & DESCRIPTION =====
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Reference")]
        public string Reference { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        // ===== TRANSFER TYPE =====
        public string TransferType { get; set; } // "internal" or "external"

        // ===== EXTERNAL TRANSFER FIELDS =====
        [Display(Name = "Recipient Name")]
        public string RecipientName { get; set; }

        [Display(Name = "Account Number")]
        public string RecipientAccount { get; set; }

        [Display(Name = "Bank")]
        public string RecipientBank { get; set; }

        [Display(Name = "External Reference")]
        public string ExternalReference { get; set; }

        public int? BeneficiaryId { get; set; }

        // ===== ADD TO BENEFICIARIES =====
        public bool AddToBeneficiaries { get; set; }

        // ===== LISTS =====
        public List<Account> UserAccounts { get; set; }
        public List<Beneficiary> Beneficiaries { get; set; }
        public decimal FromAccountBalance { get; set; }
        public string FromAccountNumber { get; set; }
    }

    public class TransferConfirmationViewModel
    {
        public string TransferType { get; set; } // "internal" or "external"

        // Internal
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public string ToAccountName { get; set; }

        // External
        public string RecipientName { get; set; }
        public string RecipientAccount { get; set; }
        public string RecipientBank { get; set; }

        // Shared
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string TransactionDate { get; set; }
        public decimal NewBalance { get; set; }
        public string TransactionReference { get; set; }
        public string Description { get; set; }
    }
}