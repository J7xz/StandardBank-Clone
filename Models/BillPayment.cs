using System;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StandardBank.Models
{
    public class BillPayment
    {
        public int Id { get; set; }

        [Required]
        public string BillType { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string ReferenceNumber { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Completed";

        public string Description { get; set; }

        // Foreign Keys - ADD THESE ATTRIBUTES
        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("Account")]
        public int AccountId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Account Account { get; set; }
    }
}