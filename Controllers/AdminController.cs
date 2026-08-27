using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using StandardBank.Models;
using StandardBank.ViewModels;

namespace StandardBank.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            var model = new AdminDashboardViewModel();

            // User stats
            model.TotalUsers = db.Users.Count();
            model.ActiveUsers = db.Users.Count(u => u.IsActive);
            model.LockedUsers = db.Users.Count(u => u.LockoutEnabled && u.LockoutEndDateUtc > DateTime.UtcNow);

            // Account stats
            model.TotalAccounts = db.Accounts.Count();
            model.TotalBalance = db.Accounts.Sum(a => a.Balance);

            // Transaction stats
            model.TotalTransactions = db.Transactions.Count();
            model.TodayTransactions = db.Transactions
                .Where(t => DbFunctions.TruncateTime(t.TransactionDate) == DateTime.Today)
                .Sum(t => t.Amount);

            // Today registrations
            model.TodayRegistrations = db.Users
                .Count(u => DbFunctions.TruncateTime(u.DateRegistered) == DateTime.Today);

            // Recent transactions (last 10)
            model.RecentTransactions = db.Transactions
                .Include(t => t.Account)
                .Include(t => t.Account.User)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .ToList();

            // Recent users (last 10)
            model.RecentUsers = db.Users
                .OrderByDescending(u => u.DateRegistered)
                .Take(10)
                .ToList();

            // Recent admin logs (last 10)
            model.RecentLogs = db.AdminLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(10)
                .ToList();

            return View(model);
        }

        // GET: Admin/Users
        // GET: Admin/Users
        // GET: Admin/Users
        public ActionResult Users(string searchTerm, string filterStatus)
        {
            var model = new UserManagementViewModel();

            var query = db.Users.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.FullName.Contains(searchTerm) ||
                                         u.Email.Contains(searchTerm) ||
                                         u.PhoneNumber.Contains(searchTerm) ||
                                         u.IDNumber.Contains(searchTerm));
            }

            // Status filter
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "Active")
                {
                    query = query.Where(u => u.IsActive && !u.LockoutEnabled);
                }
                else if (filterStatus == "Locked")
                {
                    query = query.Where(u => u.LockoutEnabled && u.LockoutEndDateUtc > DateTime.UtcNow);
                }
            }

            // Map to UserViewModel
            model.Users = query.OrderByDescending(u => u.DateRegistered)
                .ToList()
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IDNumber = u.IDNumber,
                    DateRegistered = u.DateRegistered.ToString("dd MMM yyyy"),
                    IsActive = u.IsActive,
                    IsLocked = u.LockoutEnabled && u.LockoutEndDateUtc > DateTime.UtcNow,
                    TotalAccounts = db.Accounts.Count(a => a.UserId == u.Id),
                    TotalBalance = db.Accounts.Where(a => a.UserId == u.Id).Sum(a => (decimal?)a.Balance) ?? 0,
                    LastLoginDate = u.LastLoginDate?.ToString("dd MMM yyyy HH:mm") ?? "Never"
                })
                .ToList();

            model.TotalUsers = db.Users.Count();
            model.ActiveUsers = db.Users.Count(u => u.IsActive);
            model.LockedUsers = db.Users.Count(u => u.LockoutEnabled && u.LockoutEndDateUtc > DateTime.UtcNow);
            model.SearchTerm = searchTerm;
            model.FilterStatus = filterStatus;

            return View(model);


        }

        // GET: Admin/UserDetails/{id}
        public async Task<ActionResult> UserDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new UserDetailsViewModel
            {
                User = user,
                Accounts = db.Accounts.Where(a => a.UserId == user.Id).ToList(),
                RecentTransactions = db.Transactions
                    .Include(t => t.Account)
                    .Where(t => t.Account.UserId == user.Id)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(20)
                    .ToList(),
                TotalBalance = db.Accounts.Where(a => a.UserId == user.Id).Sum(a => a.Balance),
                TotalTransactions = db.Transactions.Count(t => t.Account.UserId == user.Id)
            };

            return View(model);
        }

        // POST: Admin/LockUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LockUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userManager = new UserManager<User>(new UserStore<User>(db));
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Lock the user for 30 days
            user.LockoutEndDateUtc = DateTime.UtcNow.AddDays(30);
            user.LockoutEnabled = true;
            user.IsActive = false;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Log the action
                LogAdminAction("LockUser", id, $"User {user.FullName} was locked by admin");
                TempData["SuccessMessage"] = $"User {user.FullName} has been locked.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to lock user.";
            }

            return RedirectToAction("Users");
        }

        // POST: Admin/UnlockUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnlockUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userManager = new UserManager<User>(new UserStore<User>(db));
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Unlock the user
            user.LockoutEndDateUtc = null;
            user.LockoutEnabled = false;
            user.IsActive = true;
            user.AccessFailedCount = 0;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Log the action
                LogAdminAction("UnlockUser", id, $"User {user.FullName} was unlocked by admin");
                TempData["SuccessMessage"] = $"User {user.FullName} has been unlocked.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to unlock user.";
            }

            return RedirectToAction("Users");
        }

        // GET: Admin/Transactions
        // GET: Admin/Transactions
        public ActionResult Transactions(TransactionMonitorFilterViewModel filter)
        {
            var model = new TransactionMonitoringViewModel();

            var query = db.Transactions
                .Include(t => t.Account)
                .Include(t => t.Account.User)
                .AsQueryable();

            // Apply filters
            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    var toDate = filter.ToDate.Value.AddDays(1);
                    query = query.Where(t => t.TransactionDate < toDate);
                }

                if (!string.IsNullOrEmpty(filter.TransactionType))
                {
                    query = query.Where(t => t.TransactionType == filter.TransactionType);
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(t => t.Status == filter.Status);
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(t => t.Description.Contains(filter.SearchTerm) ||
                                             t.ReferenceNumber.Contains(filter.SearchTerm) ||
                                             t.Account.AccountNumber.Contains(filter.SearchTerm));
                }
            }

            model.Transactions = query
                .OrderByDescending(t => t.TransactionDate)
                .Take(200)
                .ToList();

            model.TotalTransactions = model.Transactions.Count();
            model.TotalAmount = model.Transactions.Sum(t => t.Amount);
            model.Filter = filter ?? new TransactionMonitorFilterViewModel();

            PopulateFilterDropdowns(model.Filter);

            return View(model);
        }

        // POST: Admin/DeleteTransaction/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteTransaction(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var transaction = await db.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }

            var description = transaction.Description;
            var amount = transaction.Amount;

            db.Transactions.Remove(transaction);
            await db.SaveChangesAsync();

            // Log the action
            LogAdminAction("DeleteTransaction", null, $"Deleted transaction #{id} - {description} - Amount: {amount.ToString("C")}");

            TempData["SuccessMessage"] = $"Transaction {id} has been deleted.";

            return RedirectToAction("Transactions");
        }

        // GET: Admin/Logs
        public ActionResult Logs(int page = 1, int pageSize = 50)
        {
            var logs = db.AdminLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalLogs = db.AdminLogs.Count();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(logs);
        }

        // POST: Admin/ClearLogs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ClearLogs()
        {
            var count = db.AdminLogs.Count();
            db.AdminLogs.RemoveRange(db.AdminLogs);
            await db.SaveChangesAsync();

            // Log the action (this will be the last log before everything is cleared)
            LogAdminAction("ClearLogs", null, $"Cleared {count} admin logs");

            TempData["SuccessMessage"] = $"All {count} logs have been cleared.";
            return RedirectToAction("Logs");
        }

        // GET: Admin/Reports
        public ActionResult Reports()
        {
            return View();
        }

        // GET: Admin/GenerateReport
        public async Task<ActionResult> GenerateReport(string reportType, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                TempData["ErrorMessage"] = "Please select both from and to dates.";
                return RedirectToAction("Reports");
            }

            // Log the action
            LogAdminAction("GenerateReport", null, $"Generated {reportType} report from {fromDate:dd MMM yyyy} to {toDate:dd MMM yyyy}");

            ViewBag.ReportType = reportType;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            var transactions = db.Transactions
                .Include(t => t.Account)
                .Include(t => t.Account.User)
                .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            ViewBag.Transactions = transactions;
            ViewBag.TotalAmount = transactions.Sum(t => t.Amount);
            ViewBag.TotalTransactions = transactions.Count();

            return View();
        }

        // Helper method to log admin actions
        private void LogAdminAction(string action, string targetUserId, string details)
        {
            var adminId = User.Identity.GetUserId();

            var log = new AdminLog
            {
                Action = action,
                UserId = adminId,
                TargetUserId = targetUserId,
                Details = details,
                Timestamp = DateTime.Now,
                IPAddress = Request.UserHostAddress,
                UserAgent = Request.UserAgent
            };

            db.AdminLogs.Add(log);
            db.SaveChanges();
        }

        // Helper method to populate filter dropdowns
        private void PopulateFilterDropdowns(TransactionMonitorFilterViewModel filter)
        {
            filter.TransactionTypes = new System.Collections.Generic.List<string>
            {
                "Debit",
                "Credit"
            };

            filter.TransactionCategories = new System.Collections.Generic.List<string>
            {
                "Transfer",
                "BillPayment",
                "Deposit",
                "Withdrawal"
            };

            filter.Statuses = new System.Collections.Generic.List<string>
            {
                "Completed",
                "Pending",
                "Failed"
            };

            filter.Users = db.Users.OrderBy(u => u.FullName).ToList();
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