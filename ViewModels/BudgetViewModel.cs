using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StandardBank.Models;

namespace StandardBank.ViewModels
{
    public class BudgetViewModel
    {
        public List<Budget> Budgets { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalRemaining { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public List<BudgetCategorySummary> CategorySummaries { get; set; }
    }

    public class BudgetCategorySummary
    {
        public string Category { get; set; }
        public decimal Allocated { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentageUsed { get; set; }
    }

    public class CreateBudgetViewModel
    {
        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Allocated Amount")]
        public decimal AllocatedAmount { get; set; }

        [Display(Name = "Spent Amount")]
        public decimal SpentAmount { get; set; } = 0;

        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Now.Month;

        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Now.Year;

        public List<string> BudgetCategories { get; set; }
    }

    public class EditBudgetViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Allocated Amount")]
        public decimal AllocatedAmount { get; set; }

        [Display(Name = "Spent Amount")]
        public decimal SpentAmount { get; set; }

        [Display(Name = "Month")]
        public int Month { get; set; }

        [Display(Name = "Year")]
        public int Year { get; set; }

        public List<string> BudgetCategories { get; set; }
    }
}