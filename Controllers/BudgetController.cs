using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using StandardBank.Models;
using StandardBank.ViewModels;

namespace StandardBank.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Budget/Index
        public ActionResult Index(int? month, int? year)
        {
            var userId = User.Identity.GetUserId();
            var currentMonth = month ?? DateTime.Now.Month;
            var currentYear = year ?? DateTime.Now.Year;

            var budgets = db.Budgets
                .Where(b => b.UserId == userId && b.Month == currentMonth && b.Year == currentYear && b.IsActive)
                .ToList();

            // Calculate spending for each budget category
            var transactions = db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionType == "Debit" &&
                           t.TransactionDate.Month == currentMonth &&
                           t.TransactionDate.Year == currentYear)
                .ToList();

            foreach (var budget in budgets)
            {
                // Update spent amount based on transactions
                budget.SpentAmount = transactions
                    .Where(t => t.TransactionCategory == budget.Category)
                    .Sum(t => t.Amount);
            }

            var model = new BudgetViewModel
            {
                Budgets = budgets,
                TotalAllocated = budgets.Sum(b => b.AllocatedAmount),
                TotalSpent = budgets.Sum(b => b.SpentAmount),
                TotalRemaining = budgets.Sum(b => b.RemainingAmount),
                CurrentMonth = currentMonth,
                CurrentYear = currentYear,
                CategorySummaries = budgets.Select(b => new BudgetCategorySummary
                {
                    Category = b.Category,
                    Allocated = b.AllocatedAmount,
                    Spent = b.SpentAmount,
                    Remaining = b.RemainingAmount,
                    PercentageUsed = b.SpentPercentage
                }).ToList()
            };

            return View(model);
        }

        // GET: Budget/Create
        public ActionResult Create(int? month, int? year)
        {
            var model = new CreateBudgetViewModel
            {
                Month = month ?? DateTime.Now.Month,
                Year = year ?? DateTime.Now.Year,
                BudgetCategories = GetBudgetCategoriesList()
            };

            return View(model);
        }

        // POST: Budget/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateBudgetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.BudgetCategories = GetBudgetCategoriesList();
                return View(model);
            }

            var userId = User.Identity.GetUserId();

            // Check if budget already exists for this category and month
            var existing = await db.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId &&
                                         b.Category == model.Category &&
                                         b.Month == model.Month &&
                                         b.Year == model.Year);

            if (existing != null)
            {
                ModelState.AddModelError("", $"A budget for '{model.Category}' already exists for {model.Month}/{model.Year}.");
                model.BudgetCategories = GetBudgetCategoriesList();
                return View(model);
            }

            var budget = new Budget
            {
                Category = model.Category,
                AllocatedAmount = model.AllocatedAmount,
                SpentAmount = 0,
                Month = model.Month,
                Year = model.Year,
                DateCreated = DateTime.Now,
                IsActive = true,
                UserId = userId
            };

            db.Budgets.Add(budget);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Budget for '{model.Category}' created successfully!";

            return RedirectToAction("Index", new { month = model.Month, year = model.Year });
        }

        // GET: Budget/Edit/{id}
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var budget = await db.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                return HttpNotFound();
            }

            var model = new EditBudgetViewModel
            {
                Id = budget.Id,
                Category = budget.Category,
                AllocatedAmount = budget.AllocatedAmount,
                SpentAmount = budget.SpentAmount,
                Month = budget.Month,
                Year = budget.Year,
                BudgetCategories = GetBudgetCategoriesList()
            };

            return View(model);
        }

        // POST: Budget/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditBudgetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.BudgetCategories = GetBudgetCategoriesList();
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            var budget = await db.Budgets
                .FirstOrDefaultAsync(b => b.Id == model.Id && b.UserId == userId);

            if (budget == null)
            {
                return HttpNotFound();
            }

            // Check if another budget exists with same category/month/year (excluding this one)
            var existing = await db.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId &&
                                         b.Category == model.Category &&
                                         b.Month == model.Month &&
                                         b.Year == model.Year &&
                                         b.Id != model.Id);

            if (existing != null)
            {
                ModelState.AddModelError("", $"A budget for '{model.Category}' already exists for {model.Month}/{model.Year}.");
                model.BudgetCategories = GetBudgetCategoriesList();
                return View(model);
            }

            budget.Category = model.Category;
            budget.AllocatedAmount = model.AllocatedAmount;
            budget.Month = model.Month;
            budget.Year = model.Year;

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Budget for '{budget.Category}' updated successfully!";

            return RedirectToAction("Index", new { month = model.Month, year = model.Year });
        }

        // GET: Budget/Delete/{id}
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var budget = await db.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                return HttpNotFound();
            }

            return View(budget);
        }

        // POST: Budget/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            var budget = await db.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
            {
                return HttpNotFound();
            }

            var month = budget.Month;
            var year = budget.Year;

            db.Budgets.Remove(budget);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Budget for '{budget.Category}' deleted successfully!";

            return RedirectToAction("Index", new { month = month, year = year });
        }

        // GET: Budget/Insights
        public ActionResult Insights(int? month, int? year)
        {
            var userId = User.Identity.GetUserId();
            var currentMonth = month ?? DateTime.Now.Month;
            var currentYear = year ?? DateTime.Now.Year;

            var budgets = db.Budgets
                .Where(b => b.UserId == userId && b.Month == currentMonth && b.Year == currentYear && b.IsActive)
                .ToList();

            // Calculate spending for each budget category
            var transactions = db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionType == "Debit" &&
                           t.TransactionDate.Month == currentMonth &&
                           t.TransactionDate.Year == currentYear)
                .ToList();

            foreach (var budget in budgets)
            {
                budget.SpentAmount = transactions
                    .Where(t => t.TransactionCategory == budget.Category)
                    .Sum(t => t.Amount);
            }

            var insights = new List<BudgetInsightViewModel>();

            foreach (var budget in budgets)
            {
                if (budget.SpentPercentage > 100)
                {
                    insights.Add(new BudgetInsightViewModel
                    {
                        Category = budget.Category,
                        Message = $"⚠️ You've exceeded your {budget.Category} budget by {budget.SpentPercentage - 100:F0}%!",
                        Severity = "danger",
                        Spent = budget.SpentAmount,
                        Allocated = budget.AllocatedAmount,
                        Percentage = budget.SpentPercentage
                    });
                }
                else if (budget.SpentPercentage >= 80)
                {
                    insights.Add(new BudgetInsightViewModel
                    {
                        Category = budget.Category,
                        Message = $"⚠️ You're at {budget.SpentPercentage:F0}% of your {budget.Category} budget. Consider reducing spending.",
                        Severity = "warning",
                        Spent = budget.SpentAmount,
                        Allocated = budget.AllocatedAmount,
                        Percentage = budget.SpentPercentage
                    });
                }
                else if (budget.SpentPercentage <= 30 && budget.SpentAmount > 0)
                {
                    insights.Add(new BudgetInsightViewModel
                    {
                        Category = budget.Category,
                        Message = $"✅ You're on track! Only {budget.SpentPercentage:F0}% of your {budget.Category} budget used.",
                        Severity = "success",
                        Spent = budget.SpentAmount,
                        Allocated = budget.AllocatedAmount,
                        Percentage = budget.SpentPercentage
                    });
                }
            }

            // Add AI spending insights
            var aiInsights = GenerateAIInsights(transactions, budgets);

            ViewBag.Insights = insights;
            ViewBag.AIInsights = aiInsights;
            ViewBag.Month = currentMonth;
            ViewBag.Year = currentYear;

            return View();
        }

        // GET: Budget/Summary
        public ActionResult Summary(int? year)
        {
            var userId = User.Identity.GetUserId();
            var currentYear = year ?? DateTime.Now.Year;

            var monthlyData = new List<MonthlyBudgetSummary>();

            for (int month = 1; month <= 12; month++)
            {
                var budgets = db.Budgets
                    .Where(b => b.UserId == userId && b.Month == month && b.Year == currentYear && b.IsActive)
                    .ToList();

                var transactions = db.Transactions
                    .Where(t => t.Account.UserId == userId &&
                               t.TransactionType == "Debit" &&
                               t.TransactionDate.Month == month &&
                               t.TransactionDate.Year == currentYear)
                    .ToList();

                foreach (var budget in budgets)
                {
                    budget.SpentAmount = transactions
                        .Where(t => t.TransactionCategory == budget.Category)
                        .Sum(t => t.Amount);
                }

                monthlyData.Add(new MonthlyBudgetSummary
                {
                    Month = month,
                    TotalAllocated = budgets.Sum(b => b.AllocatedAmount),
                    TotalSpent = budgets.Sum(b => b.SpentAmount),
                    BudgetCount = budgets.Count
                });
            }

            ViewBag.Year = currentYear;
            ViewBag.MonthlyData = monthlyData;

            return View();
        }

        // GET: Budget/GetBudgetCategories
        public JsonResult GetBudgetCategories()
        {
            return Json(GetBudgetCategoriesList(), JsonRequestBehavior.AllowGet);
        }

        // GET: Budget/CheckBudgetAlert
        public JsonResult CheckBudgetAlert()
        {
            var userId = User.Identity.GetUserId();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var budgets = db.Budgets
                .Where(b => b.UserId == userId && b.Month == currentMonth && b.Year == currentYear && b.IsActive)
                .ToList();

            var transactions = db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionType == "Debit" &&
                           t.TransactionDate.Month == currentMonth &&
                           t.TransactionDate.Year == currentYear)
                .ToList();

            foreach (var budget in budgets)
            {
                budget.SpentAmount = transactions
                    .Where(t => t.TransactionCategory == budget.Category)
                    .Sum(t => t.Amount);
            }

            var alerts = budgets
                .Where(b => b.SpentPercentage >= 80)
                .Select(b => new
                {
                    category = b.Category,
                    spent = b.SpentAmount,
                    allocated = b.AllocatedAmount,
                    percentage = b.SpentPercentage,
                    message = b.SpentPercentage > 100 ?
                        $"You've exceeded your {b.Category} budget!" :
                        $"You're at {b.SpentPercentage:F0}% of your {b.Category} budget"
                })
                .ToList();

            return Json(alerts, JsonRequestBehavior.AllowGet);
        }

        // Helper method to get budget categories - SINGLE METHOD
        private List<string> GetBudgetCategoriesList()
        {
            return new List<string>
            {
                "Groceries",
                "Transport",
                "Entertainment",
                "Utilities",
                "Rent/Mortgage",
                "Insurance",
                "Healthcare",
                "Education",
                "Shopping",
                "Dining Out",
                "Savings",
                "Investments",
                "Other"
            };
        }

        // Helper method to generate AI insights - FULLY FIXED
        private List<AIInsightViewModel> GenerateAIInsights(List<Transaction> transactions, List<Budget> budgets)
        {
            var userId = User.Identity.GetUserId();
            var insights = new List<AIInsightViewModel>();

            // Calculate total spending
            var totalSpent = transactions.Sum(t => t.Amount);

            if (transactions.Count > 0)
            {
                // Find highest spending category
                var categorySpending = transactions
                    .GroupBy(t => t.TransactionCategory)
                    .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                    .OrderByDescending(g => g.Total)
                    .FirstOrDefault();

                if (categorySpending != null)
                {
                    insights.Add(new AIInsightViewModel
                    {
                        Type = "Spending",
                        Message = $"📊 You spent the most on '{categorySpending.Category}' this month (R{categorySpending.Total:F2}).",
                        Icon = "fa-chart-line"
                    });
                }

                // Compare to previous month - CALCULATE DATES OUTSIDE LINQ QUERY
                var lastMonth = DateTime.Now.AddMonths(-1);
                var lastMonthValue = lastMonth.Month;
                var lastYearValue = lastMonth.Year;

                var previousMonthTransactions = db.Transactions
                    .Where(t => t.Account.UserId == userId &&
                               t.TransactionType == "Debit" &&
                               t.TransactionDate.Month == lastMonthValue &&
                               t.TransactionDate.Year == lastYearValue)
                    .ToList();

                if (previousMonthTransactions.Any())
                {
                    var previousTotal = previousMonthTransactions.Sum(t => t.Amount);
                    var percentChange = ((totalSpent - previousTotal) / previousTotal) * 100;

                    if (percentChange > 10)
                    {
                        insights.Add(new AIInsightViewModel
                        {
                            Type = "Trend",
                            Message = $"📈 Your spending increased by {percentChange:F0}% compared to last month.",
                            Icon = "fa-arrow-up"
                        });
                    }
                    else if (percentChange < -10)
                    {
                        insights.Add(new AIInsightViewModel
                        {
                            Type = "Trend",
                            Message = $"📉 Your spending decreased by {Math.Abs(percentChange):F0}% compared to last month. Great job!",
                            Icon = "fa-arrow-down"
                        });
                    }
                }

                // Check for spending patterns
                var weekendSpending = transactions
                    .Where(t => t.TransactionDate.DayOfWeek == DayOfWeek.Saturday ||
                               t.TransactionDate.DayOfWeek == DayOfWeek.Sunday)
                    .Sum(t => t.Amount);

                if (weekendSpending > 0)
                {
                    insights.Add(new AIInsightViewModel
                    {
                        Type = "Pattern",
                        Message = $"🛍️ You spent R{weekendSpending:F2} on weekends. Consider planning your weekend activities.",
                        Icon = "fa-calendar-week"
                    });
                }
            }

            // Add budget related insights
            if (budgets.Any())
            {
                var overBudget = budgets.Where(b => b.SpentPercentage > 100).Count();
                var atRisk = budgets.Where(b => b.SpentPercentage >= 80 && b.SpentPercentage <= 100).Count();

                if (overBudget > 0)
                {
                    insights.Add(new AIInsightViewModel
                    {
                        Type = "Alert",
                        Message = $"⚠️ You have {overBudget} category(ies) over budget. Review your spending!",
                        Icon = "fa-exclamation-triangle"
                    });
                }

                if (atRisk > 0)
                {
                    insights.Add(new AIInsightViewModel
                    {
                        Type = "Alert",
                        Message = $"⚠️ {atRisk} category(ies) are at 80%+ of budget. You're close to exceeding!",
                        Icon = "fa-clock"
                    });
                }
            }

            return insights;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Additional ViewModels for Budget
    public class BudgetInsightViewModel
    {
        public string Category { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; } // success, warning, danger
        public decimal Spent { get; set; }
        public decimal Allocated { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AIInsightViewModel
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Icon { get; set; }
    }

    public class MonthlyBudgetSummary
    {
        public int Month { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal TotalSpent { get; set; }
        public int BudgetCount { get; set; }
    }
}