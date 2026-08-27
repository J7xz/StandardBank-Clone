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
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard/Index
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user's accounts
            var accounts = await db.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            // Get recent transactions (last 5)
            var recentTransactions = await db.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToListAsync();

            // Get savings goals
            var savingsGoals = await db.SavingsGoals
                .Where(s => s.UserId == userId && !s.IsCompleted)
                .OrderBy(s => s.Deadline)
                .Take(3)
                .ToListAsync();

            // Get current month budgets
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var budgets = await db.Budgets
                .Where(b => b.UserId == userId && b.Month == currentMonth && b.Year == currentYear && b.IsActive)
                .ToListAsync();

            // Calculate spent amounts for budgets
            var transactions = await db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionType == "Debit" &&
                           t.TransactionDate.Month == currentMonth &&
                           t.TransactionDate.Year == currentYear)
                .ToListAsync();

            foreach (var budget in budgets)
            {
                budget.SpentAmount = transactions
                    .Where(t => t.TransactionCategory == budget.Category)
                    .Sum(t => t.Amount);
            }

            var model = new DashboardViewModel
            {
                FullName = user.FullName,
                TotalBalance = accounts.Sum(a => a.Balance),
                Accounts = accounts,
                RecentTransactions = recentTransactions,
                SavingsGoals = savingsGoals,
                Budgets = budgets,
                TotalAccounts = accounts.Count,
                RecentTransactionsCount = recentTransactions.Count
            };

            return View(model);
        }

        // GET: Dashboard/QuickActions
        public ActionResult QuickActions()
        {
            return PartialView("_QuickActions");
        }

        // GET: Dashboard/AccountSummary
        public async Task<ActionResult> AccountSummary()
        {
            var userId = User.Identity.GetUserId();
            var accounts = await db.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            return PartialView("_AccountSummary", accounts);
        }

        // GET: Dashboard/RecentTransactions
        public async Task<ActionResult> RecentTransactions()
        {
            var userId = User.Identity.GetUserId();
            var transactions = await db.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .ToListAsync();

            return PartialView("_RecentTransactions", transactions);
        }

        // GET: Dashboard/SavingsProgress
        public async Task<ActionResult> SavingsProgress()
        {
            var userId = User.Identity.GetUserId();
            var goals = await db.SavingsGoals
                .Where(s => s.UserId == userId && !s.IsCompleted)
                .OrderBy(s => s.Deadline)
                .ToListAsync();

            return PartialView("_SavingsProgress", goals);
        }

        // GET: Dashboard/BudgetSummary
        public async Task<ActionResult> BudgetSummary()
        {
            var userId = User.Identity.GetUserId();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var budgets = await db.Budgets
                .Where(b => b.UserId == userId && b.Month == currentMonth && b.Year == currentYear && b.IsActive)
                .ToListAsync();

            var transactions = await db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionType == "Debit" &&
                           t.TransactionDate.Month == currentMonth &&
                           t.TransactionDate.Year == currentYear)
                .ToListAsync();

            foreach (var budget in budgets)
            {
                budget.SpentAmount = transactions
                    .Where(t => t.TransactionCategory == budget.Category)
                    .Sum(t => t.Amount);
            }

            return PartialView("_BudgetSummary", budgets);
        }

        // GET: Dashboard/SpendingChart
        public async Task<JsonResult> SpendingChart()
        {
            var userId = User.Identity.GetUserId();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var transactions = await db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionType == "Debit" &&
                           t.TransactionDate.Month == currentMonth &&
                           t.TransactionDate.Year == currentYear)
                .ToListAsync();

            var spendingByCategory = transactions
                .GroupBy(t => t.TransactionCategory)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(g => g.Amount)
                .ToList();

            return Json(spendingByCategory, JsonRequestBehavior.AllowGet);
        }

        // GET: Dashboard/IncomeVsExpenses
        public async Task<JsonResult> IncomeVsExpenses()
        {
            var userId = User.Identity.GetUserId();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var income = await db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionType == "Credit" &&
                           t.TransactionDate.Month == currentMonth &&
                           t.TransactionDate.Year == currentYear)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var expenses = await db.Transactions
                .Where(t => t.Account.UserId == userId &&
                           t.TransactionType == "Debit" &&
                           t.TransactionDate.Month == currentMonth &&
                           t.TransactionDate.Year == currentYear)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var savings = income - expenses;

            return Json(new { income, expenses, savings }, JsonRequestBehavior.AllowGet);
        }

        // GET: Dashboard/MonthlySpending
        public async Task<JsonResult> MonthlySpending()
        {
            var userId = User.Identity.GetUserId();
            var currentYear = DateTime.Now.Year;

            var monthlyData = new List<object>();

            for (int month = 1; month <= 12; month++)
            {
                var monthName = new DateTime(currentYear, month, 1).ToString("MMM");
                var total = await db.Transactions
                    .Where(t => t.Account.UserId == userId &&
                               t.TransactionType == "Debit" &&
                               t.TransactionDate.Month == month &&
                               t.TransactionDate.Year == currentYear)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                monthlyData.Add(new { Month = monthName, Amount = total });
            }

            return Json(monthlyData, JsonRequestBehavior.AllowGet);
        }

        // GET: Dashboard/AccountBalances
        public async Task<JsonResult> AccountBalances()
        {
            var userId = User.Identity.GetUserId();
            var accounts = await db.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .Select(a => new { a.AccountNumber, a.AccountType, a.Balance })
                .ToListAsync();

            return Json(accounts, JsonRequestBehavior.AllowGet);
        }

        // GET: Dashboard/GetNotifications
        public async Task<JsonResult> GetNotifications()
        {
            var userId = User.Identity.GetUserId();
            var notifications = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.DateSent)
                .Take(10)
                .Select(n => new { n.Id, n.Title, n.Message, n.LinkUrl, Date = n.DateSent.ToString("dd MMM yyyy HH:mm") })
                .ToListAsync();

            return Json(notifications, JsonRequestBehavior.AllowGet);
        }

        // POST: Dashboard/MarkNotificationRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MarkNotificationRead(int id)
        {
            var userId = User.Identity.GetUserId();
            var notification = await db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                notification.DateRead = DateTime.Now;
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // POST: Dashboard/MarkAllNotificationsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MarkAllNotificationsRead()
        {
            var userId = User.Identity.GetUserId();
            var notifications = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.DateRead = DateTime.Now;
            }

            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: Dashboard/UnreadCount
        public async Task<JsonResult> UnreadCount()
        {
            var userId = User.Identity.GetUserId();
            var count = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Json(new { count }, JsonRequestBehavior.AllowGet);
        }

        // GET: Dashboard/Settings
        public ActionResult Settings()
        {
            return View();
        }

        // POST: Dashboard/ToggleDarkMode
        [HttpPost]
        public JsonResult ToggleDarkMode(bool darkMode)
        {
            // Store preference in session or database
            Session["DarkMode"] = darkMode;
            return Json(new { success = true });
        }

        // GET: Dashboard/GetDarkMode
        public JsonResult GetDarkMode()
        {
            var darkMode = Session["DarkMode"] ?? false;
            return Json(new { darkMode }, JsonRequestBehavior.AllowGet);
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
}