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
    public class BillPaymentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BillPayment/Index
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var payments = db.BillPayments
                .Include(b => b.Account)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.PaymentDate)
                .Take(20)
                .ToList();

            return View(payments);
        }

        // GET: BillPayment/PayBill
        public ActionResult PayBill()
        {
            var userId = User.Identity.GetUserId();
            var accounts = db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToList();

            if (accounts.Count == 0)
            {
                TempData["ErrorMessage"] = "You don't have any active accounts to pay bills from.";
                return RedirectToAction("Index");
            }

            var model = new BillPaymentViewModel
            {
                Accounts = accounts,
                BillTypes = new List<string> { "Electricity", "Water", "DSTV", "Internet", "Municipality", "Education" }
            };

            return View(model);
        }

        // POST: BillPayment/PayBill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PayBill(BillPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                model.Accounts = db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToList();
                model.BillTypes = new List<string> { "Electricity", "Water", "DSTV", "Internet", "Municipality", "Education" };
                return View(model);
            }

            var account = await db.Accounts.FindAsync(model.AccountId);

            if (account == null)
            {
                TempData["ErrorMessage"] = "Invalid account selected.";
                return RedirectToAction("PayBill");
            }

            if (account.Balance < model.Amount)
            {
                TempData["ErrorMessage"] = "Insufficient balance to pay this bill.";
                return RedirectToAction("PayBill");
            }

            // Generate reference number
            var reference = "BILL" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(1000, 9999);

            // Debit account
            account.Balance -= model.Amount;

            var transaction = new Transaction
            {
                Amount = model.Amount,
                TransactionType = "Debit",
                TransactionCategory = "BillPayment",
                Description = $"{model.BillType} bill payment - {model.ReferenceNumber}",
                TransactionDate = DateTime.Now,
                Status = "Completed",
                BalanceAfterTransaction = account.Balance,
                ReferenceNumber = reference,
                AccountId = account.Id
            };
            db.Transactions.Add(transaction);

            // Create bill payment record
            var billPayment = new BillPayment
            {
                BillType = model.BillType,
                Amount = model.Amount,
                ReferenceNumber = model.ReferenceNumber,
                PaymentDate = DateTime.Now,
                Status = "Completed",
                Description = model.Description,
                UserId = User.Identity.GetUserId(),
                AccountId = account.Id
            };
            db.BillPayments.Add(billPayment);

            await db.SaveChangesAsync();

            // Create confirmation model
            var confirmation = new BillPaymentConfirmationViewModel
            {
                BillType = model.BillType,
                AccountNumber = account.AccountNumber,
                Amount = model.Amount,
                ReferenceNumber = model.ReferenceNumber,
                PaymentDate = DateTime.Now.ToString("dd MMM yyyy HH:mm"),
                Status = "Completed",
                NewBalance = account.Balance
            };

            TempData["BillPaymentConfirmation"] = confirmation;
            TempData["SuccessMessage"] = $"Your {model.BillType} bill payment of {model.Amount.ToString("C")} was successful!";

            return RedirectToAction("Confirmation");
        }

        // GET: BillPayment/Confirmation
        public ActionResult Confirmation()
        {
            var confirmation = TempData["BillPaymentConfirmation"] as BillPaymentConfirmationViewModel;

            if (confirmation == null)
            {
                return RedirectToAction("Index");
            }

            return View(confirmation);
        }

        // GET: BillPayment/History
        public ActionResult History(string billType, DateTime? fromDate, DateTime? toDate, string searchTerm)
        {
            var userId = User.Identity.GetUserId();
            var query = db.BillPayments
                .Include(b => b.Account)
                .Where(b => b.UserId == userId);

            // Apply bill type filter
            if (!string.IsNullOrEmpty(billType) && billType != "All")
            {
                query = query.Where(b => b.BillType == billType);
            }

            // Apply date filters
            if (fromDate.HasValue)
            {
                query = query.Where(b => b.PaymentDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(b => b.PaymentDate < endDate);
            }

            // Apply search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.BillType.Contains(searchTerm) ||
                                         b.ReferenceNumber.Contains(searchTerm) ||
                                         b.Description.Contains(searchTerm));
            }

            var payments = query
                .OrderByDescending(b => b.PaymentDate)
                .ToList();

            ViewBag.BillTypes = new List<string> { "All", "Electricity", "Water", "DSTV", "Internet", "Municipality", "Education" };
            ViewBag.SelectedBillType = billType;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.SearchTerm = searchTerm;

            return View(payments);
        }

        // GET: BillPayment/Details/{id}
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var payment = await db.BillPayments
                .Include(b => b.Account)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (payment == null)
            {
                return HttpNotFound();
            }

            return View(payment);
        }

        // GET: BillPayment/Receipt/{id}
        public async Task<ActionResult> Receipt(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var payment = await db.BillPayments
                .Include(b => b.Account)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (payment == null)
            {
                return HttpNotFound();
            }

            return View(payment);
        }

        // POST: BillPayment/SchedulePayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SchedulePayment(BillPaymentViewModel model, DateTime scheduledDate)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid payment details.";
                return RedirectToAction("PayBill");
            }

            var account = await db.Accounts.FindAsync(model.AccountId);

            if (account == null)
            {
                TempData["ErrorMessage"] = "Invalid account selected.";
                return RedirectToAction("PayBill");
            }

            // Create scheduled bill payment (status = Pending)
            var billPayment = new BillPayment
            {
                BillType = model.BillType,
                Amount = model.Amount,
                ReferenceNumber = model.ReferenceNumber,
                PaymentDate = scheduledDate,
                Status = "Pending",
                Description = $"Scheduled payment - {model.Description}",
                UserId = User.Identity.GetUserId(),
                AccountId = account.Id
            };
            db.BillPayments.Add(billPayment);

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Your {model.BillType} bill payment has been scheduled for {scheduledDate.ToString("dd MMM yyyy")}.";

            return RedirectToAction("Index");
        }

        // POST: BillPayment/CancelScheduled/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelScheduled(int id)
        {
            var userId = User.Identity.GetUserId();
            var payment = await db.BillPayments
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.Status == "Pending");

            if (payment == null)
            {
                return HttpNotFound();
            }

            db.BillPayments.Remove(payment);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Scheduled payment has been cancelled.";

            return RedirectToAction("Index");
        }

        // GET: BillPayment/GetBillerInfo
        public JsonResult GetBillerInfo(string billType)
        {
            // This would call an external API in production
            // For now, return mock data
            var billerInfo = new
            {
                name = GetBillerName(billType),
                referenceFormat = "Enter your account/reference number",
                minAmount = 10.00m,
                maxAmount = 100000.00m
            };

            return Json(billerInfo, JsonRequestBehavior.AllowGet);
        }

        // GET: BillPayment/GetBillTypes
        public JsonResult GetBillTypes()
        {
            var billTypes = new List<string> { "Electricity", "Water", "DSTV", "Internet", "Municipality", "Education" };
            return Json(billTypes, JsonRequestBehavior.AllowGet);
        }

        // Helper method to get biller name
        private string GetBillerName(string billType)
        {
            switch (billType)
            {
                case "Electricity":
                    return "Eskom";
                case "Water":
                    return "Municipal Water Services";
                case "DSTV":
                    return "MultiChoice DSTV";
                case "Internet":
                    return "Internet Service Provider";
                case "Municipality":
                    return "Municipal Services";
                case "Education":
                    return "Education Institution";
                default:
                    return billType;
            }
        }

        // GET: BillPayment/Statistics
        public ActionResult Statistics()
        {
            var userId = User.Identity.GetUserId();

            var stats = new
            {
                TotalPayments = db.BillPayments.Count(b => b.UserId == userId),
                TotalAmount = db.BillPayments.Where(b => b.UserId == userId).Sum(b => b.Amount),
                ThisMonth = db.BillPayments.Count(b => b.UserId == userId && b.PaymentDate.Month == DateTime.Now.Month && b.PaymentDate.Year == DateTime.Now.Year),
                ThisMonthAmount = db.BillPayments
                    .Where(b => b.UserId == userId && b.PaymentDate.Month == DateTime.Now.Month && b.PaymentDate.Year == DateTime.Now.Year)
                    .Sum(b => b.Amount)
            };

            return Json(stats, JsonRequestBehavior.AllowGet);
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