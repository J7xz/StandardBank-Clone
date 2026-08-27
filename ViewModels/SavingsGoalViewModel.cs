using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class SavingsGoalViewModel
    {
        public List<SavingsGoal> SavingsGoals { get; set; }
        public decimal TotalTargetAmount { get; set; }
        public decimal TotalCurrentAmount { get; set; }
        public int CompletedGoals { get; set; }
        public int ActiveGoals { get; set; }
    }

    public class CreateSavingsGoalViewModel
    {
        [Required]
        [Display(Name = "Goal Name")]
        public string GoalName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Target Amount")]
        public decimal TargetAmount { get; set; }

        [Display(Name = "Current Amount")]
        public decimal CurrentAmount { get; set; } = 0;

        [Display(Name = "Deadline")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
    }

    public class EditSavingsGoalViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Goal Name")]
        public string GoalName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Target Amount")]
        public decimal TargetAmount { get; set; }

        [Display(Name = "Current Amount")]
        public decimal CurrentAmount { get; set; }

        [Display(Name = "Deadline")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Is Completed")]
        public bool IsCompleted { get; set; }
    }
}