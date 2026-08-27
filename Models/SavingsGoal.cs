using System;
using System.ComponentModel.DataAnnotations;

namespace StandardBank.Models
{
    public class SavingsGoal
    {
        public int Id { get; set; }

        [Required]
        public string GoalName { get; set; }

        [Required]
        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; } = 0;

        public DateTime? Deadline { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public bool IsCompleted { get; set; } = false;

        public string Description { get; set; }

        // Calculated property
        public decimal ProgressPercentage
        {
            get
            {
                if (TargetAmount == 0) return 0;
                return (CurrentAmount / TargetAmount) * 100;
            }
        }

        // Foreign Key
        [Required]
        public string UserId { get; set; }

        // Navigation property
        public virtual User User { get; set; }
    }
}