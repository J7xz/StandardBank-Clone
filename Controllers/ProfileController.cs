using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using StandardBank.Models;
using StandardBank.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace StandardBank.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Profile/Index
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IDNumber = user.IDNumber,
                DateRegistered = user.DateRegistered.ToString("dd MMM yyyy"),
                LastLoginDate = user.LastLoginDate?.ToString("dd MMM yyyy HH:mm") ?? "Never",
                IsActive = user.IsActive
            };

            return View(model);
        }

        // GET: Profile/Edit
        public async Task<ActionResult> Edit()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IDNumber = user.IDNumber
            };

            return View(model);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.IDNumber = model.IDNumber;

            var result = await UserManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(model);
        }

        // GET: Profile/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            var result = await UserManager.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                TempData["SuccessMessage"] = "Your password has been changed successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(model);
        }

        // GET: Profile/ManageAccount
        public async Task<ActionResult> ManageAccount()
        {
            var userId = User.Identity.GetUserId();
            var accounts = await db.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            ViewBag.TotalBalance = accounts.Sum(a => a.Balance);
            ViewBag.AccountCount = accounts.Count;

            return View(accounts);
        }

        // GET: Profile/CloseAccount/{id}
        public async Task<ActionResult> CloseAccount(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var account = await db.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null)
            {
                return HttpNotFound();
            }

            if (account.Balance > 0)
            {
                TempData["ErrorMessage"] = "Cannot close account with remaining balance. Please withdraw all funds first.";
                return RedirectToAction("ManageAccount");
            }

            return View(account);
        }

        // POST: Profile/CloseAccount/{id}
        [HttpPost, ActionName("CloseAccount")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CloseAccountConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            var account = await db.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null)
            {
                return HttpNotFound();
            }

            account.IsActive = false;
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Account {account.AccountNumber} has been closed successfully.";
            return RedirectToAction("ManageAccount");
        }

        // GET: Profile/DeleteAccount
        public async Task<ActionResult> DeleteAccount()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var accounts = await db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToListAsync();

            if (accounts.Any(a => a.Balance > 0))
            {
                TempData["ErrorMessage"] = "Cannot delete account. Please withdraw all funds from all accounts first.";
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: Profile/DeleteAccount
        [HttpPost, ActionName("DeleteAccount")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAccountConfirmed()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Close all accounts first
            var accounts = await db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToListAsync();
            foreach (var account in accounts)
            {
                account.IsActive = false;
            }
            await db.SaveChangesAsync();

            // Delete user
            var result = await UserManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // Log out the user
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                TempData["SuccessMessage"] = "Your account has been deleted successfully.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return RedirectToAction("Index");
        }

        // GET: Profile/ActivityLog
        public async Task<ActionResult> ActivityLog(int page = 1, int pageSize = 20)
        {
            var userId = User.Identity.GetUserId();

            // Get user's transaction history
            var transactions = await db.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalTransactions = await db.Transactions
                .CountAsync(t => t.Account.UserId == userId);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTransactions / pageSize);

            return View(transactions);
        }

        // GET: Profile/NotificationPreferences
        public async Task<ActionResult> NotificationPreferences()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            // In a real app, you'd have a UserPreferences table
            // For now, just show a view with default values
            return View();
        }

        // POST: Profile/NotificationPreferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NotificationPreferences(NotificationPreferencesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // In a real app, save to database
            // For now, just show success message
            TempData["SuccessMessage"] = "Notification preferences updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: Profile/GetProfileSummary (AJAX)
        public async Task<JsonResult> GetProfileSummary()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
            }

            var accounts = await db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToListAsync();

            var summary = new
            {
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                accountCount = accounts.Count,
                totalBalance = accounts.Sum(a => a.Balance),
                dateRegistered = user.DateRegistered.ToString("dd MMM yyyy"),
                lastLogin = user.LastLoginDate?.ToString("dd MMM yyyy HH:mm") ?? "Never"
            };

            return Json(new { success = true, data = summary }, JsonRequestBehavior.AllowGet);
        }

        // GET: Profile/CheckEmailAvailability (AJAX)
        public async Task<JsonResult> CheckEmailAvailability(string email)
        {
            var userId = User.Identity.GetUserId();
            var existingUser = await UserManager.FindByEmailAsync(email);

            if (existingUser != null && existingUser.Id != userId)
            {
                return Json(new { available = false, message = "This email is already registered." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { available = true }, JsonRequestBehavior.AllowGet);
        }

        #region Helpers

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ApplicationSignInManager _signInManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set { _signInManager = value; }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }
            base.Dispose(disposing);
        }
    }

    // Additional ViewModel for Notification Preferences
    public class NotificationPreferencesViewModel
    {
        public bool EmailNotifications { get; set; } = true;
        public bool InAppNotifications { get; set; } = true;
        public bool TransactionAlerts { get; set; } = true;
        public bool BillPaymentReminders { get; set; } = true;
        public bool SavingsGoalAlerts { get; set; } = true;
        public bool BudgetAlerts { get; set; } = true;
        public bool PromotionalEmails { get; set; } = false;
        public bool SecurityAlerts { get; set; } = true;
    }
}