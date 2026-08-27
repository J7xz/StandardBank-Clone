using Microsoft.AspNet.Identity;
using StandardBank.Models;
using StandardBank.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace StandardBank.Controllers
{
    [Authorize]
    public class SavingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Savings/Index
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();

            var goals = await db.SavingsGoals
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.IsCompleted)
                .ThenBy(s => s.Deadline)
                .ToListAsync();

            var model = new SavingsGoalViewModel
            {
                SavingsGoals = goals,
                TotalTargetAmount = goals.Sum(g => g.TargetAmount),
                TotalCurrentAmount = goals.Sum(g => g.CurrentAmount),
                CompletedGoals = goals.Count(g => g.IsCompleted),
                ActiveGoals = goals.Count(g => !g.IsCompleted)
            };

            return View(model);
        }

        // GET: Savings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Savings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateSavingsGoalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();

            var goal = new SavingsGoal
            {
                GoalName = model.GoalName,
                TargetAmount = model.TargetAmount,
                CurrentAmount = model.CurrentAmount,
                Deadline = model.Deadline,
                DateCreated = DateTime.Now,
                IsCompleted = false,
                Description = model.Description,
                UserId = userId
            };

            db.SavingsGoals.Add(goal);
            await db.SaveChangesAsync();

            if (model.CurrentAmount > 0)
            {
                await CreateSavingsTransaction(userId, model.CurrentAmount, $"Initial deposit for {model.GoalName}");
            }

            TempData["SuccessMessage"] = $"Savings goal '{model.GoalName}' created successfully!";

            return RedirectToAction("Index");
        }

        // GET: Savings/Edit/{id}
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            var model = new EditSavingsGoalViewModel
            {
                Id = goal.Id,
                GoalName = goal.GoalName,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                Deadline = goal.Deadline,
                Description = goal.Description,
                IsCompleted = goal.IsCompleted
            };

            return View(model);
        }

        // POST: Savings/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditSavingsGoalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == model.Id && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            goal.GoalName = model.GoalName;
            goal.TargetAmount = model.TargetAmount;
            goal.CurrentAmount = model.CurrentAmount;
            goal.Deadline = model.Deadline;
            goal.Description = model.Description;
            goal.IsCompleted = model.IsCompleted;

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Savings goal '{goal.GoalName}' updated successfully!";

            return RedirectToAction("Index");
        }

        // GET: Savings/Delete/{id}
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            return View(goal);
        }

        // POST: Savings/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            var goalName = goal.GoalName;

            db.SavingsGoals.Remove(goal);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Savings goal '{goalName}' deleted successfully!";

            return RedirectToAction("Index");
        }

        // GET: Savings/Details/{id}
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            var transactions = await db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionCategory == "Savings" &&
                           t.Description.Contains(goal.GoalName))
                .OrderByDescending(t => t.TransactionDate)
                .Take(20)
                .ToListAsync();

            ViewBag.Transactions = transactions;

            return View(goal);
        }

        // GET: Savings/AddFunds/{id}
        public async Task<ActionResult> AddFunds(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            var accounts = await db.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            var model = new AddFundsViewModel
            {
                GoalId = goal.Id,
                GoalName = goal.GoalName,
                CurrentAmount = goal.CurrentAmount,
                TargetAmount = goal.TargetAmount,
                Accounts = accounts
            };

            return View(model);
        }

        // POST: Savings/AddFunds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddFunds(AddFundsViewModel model)
        {
            var userId = User.Identity.GetUserId();

            if (!ModelState.IsValid)
            {
                model.Accounts = await db.Accounts
                    .Where(a => a.UserId == userId && a.IsActive)
                    .ToListAsync();
                return View(model);
            }

            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == model.GoalId && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            var account = await db.Accounts
                .FirstOrDefaultAsync(a => a.Id == model.AccountId && a.UserId == userId);

            if (account == null)
            {
                ModelState.AddModelError("AccountId", "Invalid account selected.");
                model.Accounts = await db.Accounts
                    .Where(a => a.UserId == userId && a.IsActive)
                    .ToListAsync();
                return View(model);
            }

            if (account.Balance < model.Amount)
            {
                ModelState.AddModelError("Amount", "Insufficient balance in selected account.");
                model.Accounts = await db.Accounts
                    .Where(a => a.UserId == userId && a.IsActive)
                    .ToListAsync();
                return View(model);
            }

            account.Balance -= model.Amount;

            var transaction = new Transaction
            {
                Amount = model.Amount,
                TransactionType = "Debit",
                TransactionCategory = "Savings",
                Description = $"Savings deposit to '{goal.GoalName}'",
                TransactionDate = DateTime.Now,
                Status = "Completed",
                BalanceAfterTransaction = account.Balance,
                ReferenceNumber = "SAV" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(1000, 9999),
                AccountId = account.Id
            };
            db.Transactions.Add(transaction);

            goal.CurrentAmount += model.Amount;

            if (goal.CurrentAmount >= goal.TargetAmount)
            {
                goal.IsCompleted = true;
                TempData["SuccessMessage"] = $"🎉 Congratulations! You've reached your savings goal '{goal.GoalName}'!";
            }

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Added {model.Amount.ToString("C")} to '{goal.GoalName}' savings goal.";

            return RedirectToAction("Index");
        }

        // GET: Savings/WithdrawFunds/{id}
        public async Task<ActionResult> WithdrawFunds(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            var accounts = await db.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            var model = new WithdrawFundsViewModel
            {
                GoalId = goal.Id,
                GoalName = goal.GoalName,
                CurrentAmount = goal.CurrentAmount,
                TargetAmount = goal.TargetAmount,
                Accounts = accounts,
                MaxAmount = goal.CurrentAmount
            };

            return View(model);
        }

        // POST: Savings/WithdrawFunds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> WithdrawFunds(WithdrawFundsViewModel model)
        {
            var userId = User.Identity.GetUserId();

            if (!ModelState.IsValid)
            {
                model.Accounts = await db.Accounts
                    .Where(a => a.UserId == userId && a.IsActive)
                    .ToListAsync();
                return View(model);
            }

            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == model.GoalId && s.UserId == userId);

            if (goal == null)
            {
                return HttpNotFound();
            }

            if (goal.CurrentAmount < model.Amount)
            {
                ModelState.AddModelError("Amount", "Insufficient funds in savings goal.");
                model.Accounts = await db.Accounts
                    .Where(a => a.UserId == userId && a.IsActive)
                    .ToListAsync();
                return View(model);
            }

            var account = await db.Accounts
                .FirstOrDefaultAsync(a => a.Id == model.AccountId && a.UserId == userId);

            if (account == null)
            {
                ModelState.AddModelError("AccountId", "Invalid account selected.");
                model.Accounts = await db.Accounts
                    .Where(a => a.UserId == userId && a.IsActive)
                    .ToListAsync();
                return View(model);
            }

            account.Balance += model.Amount;

            var transaction = new Transaction
            {
                Amount = model.Amount,
                TransactionType = "Credit",
                TransactionCategory = "Savings",
                Description = $"Savings withdrawal from '{goal.GoalName}'",
                TransactionDate = DateTime.Now,
                Status = "Completed",
                BalanceAfterTransaction = account.Balance,
                ReferenceNumber = "SAVW" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(1000, 9999),
                AccountId = account.Id
            };
            db.Transactions.Add(transaction);

            goal.CurrentAmount -= model.Amount;

            if (goal.IsCompleted && goal.CurrentAmount < goal.TargetAmount)
            {
                goal.IsCompleted = false;
            }

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Withdrew {model.Amount.ToString("C")} from '{goal.GoalName}' savings goal.";

            return RedirectToAction("Index");
        }

        // GET: Savings/Progress
        public async Task<ActionResult> Progress()
        {
            var userId = User.Identity.GetUserId();

            var goals = await db.SavingsGoals
                .Where(s => s.UserId == userId && !s.IsCompleted)
                .OrderBy(s => s.Deadline)
                .ToListAsync();

            return PartialView("_Progress", goals);
        }

        // GET: Savings/Completed
        public async Task<ActionResult> Completed()
        {
            var userId = User.Identity.GetUserId();

            var goals = await db.SavingsGoals
                .Where(s => s.UserId == userId && s.IsCompleted)
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();

            return View(goals);
        }

        // GET: Savings/DashboardWidget
        public async Task<ActionResult> DashboardWidget()
        {
            var userId = User.Identity.GetUserId();

            var goals = await db.SavingsGoals
                .Where(s => s.UserId == userId && !s.IsCompleted)
                .OrderBy(s => s.Deadline)
                .Take(3)
                .ToListAsync();

            return PartialView("_DashboardWidget", goals);
        }

        // GET: Savings/GetGoals (AJAX)
        public async Task<JsonResult> GetGoals()
        {
            var userId = User.Identity.GetUserId();

            var goals = await db.SavingsGoals
                .Where(s => s.UserId == userId)
                .Select(s => new
                {
                    s.Id,
                    s.GoalName,
                    s.TargetAmount,
                    s.CurrentAmount,
                    s.Deadline,
                    s.IsCompleted,
                    Progress = s.ProgressPercentage
                })
                .ToListAsync();

            return Json(goals, JsonRequestBehavior.AllowGet);
        }

        // GET: Savings/GetGoalProgress/{id}
        public async Task<JsonResult> GetGoalProgress(int id)
        {
            var userId = User.Identity.GetUserId();

            var goal = await db.SavingsGoals
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (goal == null)
            {
                return Json(new { success = false, message = "Goal not found" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                goalName = goal.GoalName,
                targetAmount = goal.TargetAmount,
                currentAmount = goal.CurrentAmount,
                progress = goal.ProgressPercentage,
                remaining = goal.TargetAmount - goal.CurrentAmount,
                isCompleted = goal.IsCompleted
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Savings/Summary
        public async Task<JsonResult> Summary()
        {
            var userId = User.Identity.GetUserId();

            var goals = await db.SavingsGoals
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var totalTarget = goals.Sum(g => g.TargetAmount);
            var totalCurrent = goals.Sum(g => g.CurrentAmount);
            var completedGoals = goals.Count(g => g.IsCompleted);
            var activeGoals = goals.Count(g => !g.IsCompleted);

            return Json(new
            {
                totalTarget,
                totalCurrent,
                completedGoals,
                activeGoals,
                overallProgress = totalTarget > 0 ? (totalCurrent / totalTarget) * 100 : 0
            }, JsonRequestBehavior.AllowGet);
        }

        private async Task CreateSavingsTransaction(string userId, decimal amount, string description)
        {
            var account = await db.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId && a.AccountType == "Savings" && a.IsActive);

            if (account != null)
            {
                account.Balance += amount;

                var transaction = new Transaction
                {
                    Amount = amount,
                    TransactionType = "Credit",
                    TransactionCategory = "Savings",
                    Description = description,
                    TransactionDate = DateTime.Now,
                    Status = "Completed",
                    BalanceAfterTransaction = account.Balance,
                    ReferenceNumber = "SAVINIT" + DateTime.Now.ToString("yyMMddHHmmss"),
                    AccountId = account.Id
                };

                db.Transactions.Add(transaction);
                await db.SaveChangesAsync();
            }
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

    // Additional ViewModels for Savings
    public class AddFundsViewModel
    {
        public int GoalId { get; set; }
        public string GoalName { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }

        [Required]
        [Display(Name = "Account to withdraw from")]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount to add")]
        public decimal Amount { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public List<Account> Accounts { get; set; }
    }

    public class WithdrawFundsViewModel
    {
        public int GoalId { get; set; }
        public string GoalName { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal MaxAmount { get; set; }

        [Required]
        [Display(Name = "Account to deposit to")]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount to withdraw")]
        public decimal Amount { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public List<Account> Accounts { get; set; }
    }
}