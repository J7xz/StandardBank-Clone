using System;
using System.ComponentModel.DataAnnotations;

namespace StandardBank.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public decimal AllocatedAmount { get; set; }

        public decimal SpentAmount { get; set; } = 0;

        public int Month { get; set; }

        public int Year { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Calculated properties
        public decimal RemainingAmount => AllocatedAmount - SpentAmount;

        public decimal SpentPercentage
        {
            get
            {
                if (AllocatedAmount == 0) return 0;
                return (SpentAmount / AllocatedAmount) * 100;
            }
        }

        // Foreign Key
        [Required]
        public string UserId { get; set; }

        // Navigation property
        public virtual User User { get; set; }
    }
}