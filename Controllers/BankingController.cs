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
    public class BankingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Banking/Index
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var accounts = db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToList();

            var model = new AccountViewModel
            {
                Accounts = accounts,
                TotalBalance = accounts.Sum(a => a.Balance),
                ActiveAccounts = accounts.Count(a => a.IsActive)
            };

            return View(model);
        }

        // GET: Banking/Transfer
        public ActionResult Transfer()
        {
            var userId = User.Identity.GetUserId();
            var accounts = db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToList();

            var model = new TransferViewModel
            {
                UserAccounts = accounts,
                Beneficiaries = db.Beneficiaries.Where(b => b.UserId == userId && b.IsActive).ToList(),
                TransferType = "internal"
            };

            return View(model);
        }

        // POST: Banking/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Transfer(TransferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                model.UserAccounts = await db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToListAsync();
                model.Beneficiaries = await db.Beneficiaries.Where(b => b.UserId == userId && b.IsActive).ToListAsync();
                return View(model);
            }

            var currentUserId = User.Identity.GetUserId();
            var fromAccount = await db.Accounts.FindAsync(model.FromAccountId);

            if (fromAccount == null || fromAccount.UserId != currentUserId)
            {
                TempData["ErrorMessage"] = "Invalid account selected.";
                return RedirectToAction("Transfer");
            }

            if (fromAccount.Balance < model.Amount)
            {
                TempData["ErrorMessage"] = "Insufficient balance.";
                return RedirectToAction("Transfer");
            }

            // Generate reference number
            var reference = "TXN" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(1000, 9999);

            // ===== INTERNAL TRANSFER =====
            if (model.TransferType == "internal")
            {
                var toAccount = await db.Accounts.FindAsync(model.ToAccountId);

                if (toAccount == null)
                {
                    TempData["ErrorMessage"] = "Invalid destination account.";
                    return RedirectToAction("Transfer");
                }

                if (toAccount.UserId != currentUserId)
                {
                    TempData["ErrorMessage"] = "You can only transfer to your own accounts.";
                    return RedirectToAction("Transfer");
                }

                if (fromAccount.Id == toAccount.Id)
                {
                    TempData["ErrorMessage"] = "Cannot transfer to the same account.";
                    return RedirectToAction("Transfer");
                }

                // Debit from account
                fromAccount.Balance -= model.Amount;

                var debitTransaction = new Transaction
                {
                    Amount = model.Amount,
                    TransactionType = "Debit",
                    TransactionCategory = "Transfer",
                    Description = $"Transfer to {toAccount.AccountNumber} - {model.Description}",
                    TransactionDate = DateTime.Now,
                    Status = "Completed",
                    BalanceAfterTransaction = fromAccount.Balance,
                    ReferenceNumber = reference,
                    AccountId = fromAccount.Id
                };
                db.Transactions.Add(debitTransaction);

                // Credit to account
                toAccount.Balance += model.Amount;

                var creditTransaction = new Transaction
                {
                    Amount = model.Amount,
                    TransactionType = "Credit",
                    TransactionCategory = "Transfer",
                    Description = $"Transfer from {fromAccount.AccountNumber} - {model.Description}",
                    TransactionDate = DateTime.Now,
                    Status = "Completed",
                    BalanceAfterTransaction = toAccount.Balance,
                    ReferenceNumber = reference,
                    AccountId = toAccount.Id
                };
                db.Transactions.Add(creditTransaction);

                await db.SaveChangesAsync();

                var confirmation = new TransferConfirmationViewModel
                {
                    TransferType = "internal",
                    FromAccountNumber = fromAccount.AccountNumber,
                    ToAccountNumber = toAccount.AccountNumber,
                    ToAccountName = toAccount.AccountName ?? "Recipient Account",
                    Amount = model.Amount,
                    Reference = model.Reference,
                    TransactionDate = DateTime.Now.ToString("dd MMM yyyy HH:mm"),
                    NewBalance = fromAccount.Balance,
                    TransactionReference = reference,
                    Description = model.Description
                };

                TempData["TransferConfirmation"] = confirmation;
                TempData["SuccessMessage"] = "Internal transfer completed successfully!";
            }

            // ===== EXTERNAL TRANSFER =====
            else if (model.TransferType == "external")
            {
                // Check if beneficiary was selected OR manual entry provided
                bool hasBeneficiary = model.BeneficiaryId.HasValue && model.BeneficiaryId.Value > 0;
                bool hasManualEntry = !string.IsNullOrEmpty(model.RecipientName) &&
                                      !string.IsNullOrEmpty(model.RecipientAccount) &&
                                      !string.IsNullOrEmpty(model.RecipientBank);

                if (!hasBeneficiary && !hasManualEntry)
                {
                    TempData["ErrorMessage"] = "Please select a beneficiary or enter recipient details.";
                    return RedirectToAction("Transfer");
                }

                string recipientName = "";
                string recipientAccount = "";
                string recipientBank = "";

                // Get details from beneficiary if selected
                if (hasBeneficiary)
                {
                    var beneficiary = await db.Beneficiaries.FindAsync(model.BeneficiaryId.Value);
                    if (beneficiary != null)
                    {
                        recipientName = beneficiary.Name;
                        recipientAccount = beneficiary.AccountNumber;
                        recipientBank = beneficiary.BankName;
                    }
                }
                else
                {
                    recipientName = model.RecipientName;
                    recipientAccount = model.RecipientAccount;
                    recipientBank = model.RecipientBank;
                }

                // Debit from account only (external transfer)
                fromAccount.Balance -= model.Amount;

                var debitTransaction = new Transaction
                {
                    Amount = model.Amount,
                    TransactionType = "Debit",
                    TransactionCategory = "Transfer",
                    Description = $"External transfer to {recipientName} - {model.ExternalReference ?? model.Reference}",
                    TransactionDate = DateTime.Now,
                    Status = "Completed",
                    BalanceAfterTransaction = fromAccount.Balance,
                    ReferenceNumber = reference,
                    AccountId = fromAccount.Id
                };
                db.Transactions.Add(debitTransaction);

                await db.SaveChangesAsync();

                // Save as beneficiary if checkbox is checked and manual entry was used
                if (model.AddToBeneficiaries && !hasBeneficiary && !string.IsNullOrEmpty(recipientName) && !string.IsNullOrEmpty(recipientAccount))
                {
                    var existingBeneficiary = await db.Beneficiaries
                        .FirstOrDefaultAsync(b => b.UserId == currentUserId &&
                                                  b.AccountNumber == recipientAccount);

                    if (existingBeneficiary == null)
                    {
                        var newBeneficiary = new Beneficiary
                        {
                            Name = recipientName,
                            AccountNumber = recipientAccount,
                            BankName = recipientBank,
                            Nickname = recipientName,
                            UserId = currentUserId,
                            DateAdded = DateTime.Now,
                            IsActive = true
                        };
                        db.Beneficiaries.Add(newBeneficiary);
                        await db.SaveChangesAsync();
                    }
                }

                var confirmation = new TransferConfirmationViewModel
                {
                    TransferType = "external",
                    FromAccountNumber = fromAccount.AccountNumber,
                    RecipientName = recipientName,
                    RecipientAccount = recipientAccount,
                    RecipientBank = recipientBank,
                    Amount = model.Amount,
                    Reference = model.ExternalReference ?? model.Reference,
                    TransactionDate = DateTime.Now.ToString("dd MMM yyyy HH:mm"),
                    NewBalance = fromAccount.Balance,
                    TransactionReference = reference,
                    Description = model.Description
                };

                TempData["TransferConfirmation"] = confirmation;
                TempData["SuccessMessage"] = $"External transfer to {recipientName} completed successfully!";
            }

            return RedirectToAction("TransferConfirmation");
        }

        // GET: Banking/TransferConfirmation
        public ActionResult TransferConfirmation()
        {
            var confirmation = TempData["TransferConfirmation"] as TransferConfirmationViewModel;

            if (confirmation == null)
            {
                return RedirectToAction("Transfer");
            }

            return View(confirmation);
        }

        // GET: Banking/History - FIXED with proper Include
        public async Task<ActionResult> History(int? accountId, string filterType, DateTime? fromDate, DateTime? toDate, string searchTerm)
        {
            var userId = User.Identity.GetUserId();
            var accounts = await db.Accounts.Where(a => a.UserId == userId && a.IsActive).ToListAsync();

            var query = db.Transactions
                .Include(t => t.Account)  // ✅ IMPORTANT: Include Account navigation property
                .Where(t => t.Account.UserId == userId);

            // Apply account filter
            if (accountId.HasValue && accountId.Value > 0)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }

            // Apply transaction type filter
            if (!string.IsNullOrEmpty(filterType) && filterType != "All")
            {
                query = query.Where(t => t.TransactionType == filterType);
            }

            // Apply date filters
            if (fromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(t => t.TransactionDate < endDate);
            }

            // Apply search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => t.Description.Contains(searchTerm) ||
                                         t.ReferenceNumber.Contains(searchTerm));
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            var model = new TransactionHistoryViewModel
            {
                Transactions = transactions,
                TotalDebit = transactions.Where(t => t.TransactionType == "Debit").Sum(t => t.Amount),
                TotalCredit = transactions.Where(t => t.TransactionType == "Credit").Sum(t => t.Amount),
                TotalTransactions = transactions.Count,
                FilterType = filterType,
                FromDate = fromDate,
                ToDate = toDate,
                SearchTerm = searchTerm,
                AccountId = accountId,
                Accounts = accounts,
                TransactionTypes = new List<string> { "All", "Debit", "Credit" }
            };

            return View(model);
        }

        // GET: Banking/CreateAccount
        public ActionResult CreateAccount()
        {
            var model = new CreateAccountViewModel
            {
                AccountTypes = new List<string> { "Cheque", "Savings", "Student" }
            };

            return View(model);
        }

        // POST: Banking/CreateAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAccount(CreateAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AccountTypes = new List<string> { "Cheque", "Savings", "Student" };
                return View(model);
            }

            var userId = User.Identity.GetUserId();

            var account = new Account
            {
                AccountNumber = GenerateAccountNumber(),
                AccountType = model.AccountType,
                AccountName = model.AccountName ?? $"{model.AccountType} Account",
                Balance = model.InitialDeposit,
                DateOpened = DateTime.Now,
                IsActive = true,
                UserId = userId
            };

            db.Accounts.Add(account);

            // If initial deposit > 0, create a transaction
            if (model.InitialDeposit > 0)
            {
                var transaction = new Transaction
                {
                    Amount = model.InitialDeposit,
                    TransactionType = "Credit",
                    TransactionCategory = "Deposit",
                    Description = "Initial deposit for new account",
                    TransactionDate = DateTime.Now,
                    Status = "Completed",
                    BalanceAfterTransaction = model.InitialDeposit,
                    ReferenceNumber = "INITIAL" + DateTime.Now.ToString("yyMMddHHmmss"),
                    AccountId = account.Id
                };
                db.Transactions.Add(transaction);
            }

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Your {model.AccountType} account has been created successfully! Account Number: {account.AccountNumber}";

            return RedirectToAction("Index");
        }

        // GET: Banking/AccountDetails/{id}
        public async Task<ActionResult> AccountDetails(int id)
        {
            var userId = User.Identity.GetUserId();
            var account = await db.Accounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null)
            {
                return HttpNotFound();
            }

            var transactions = account.Transactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(50)
                .ToList();

            ViewBag.Account = account;
            ViewBag.Transactions = transactions;

            return View();
        }

        // GET: Banking/Beneficiaries
        public async Task<ActionResult> Beneficiaries()
        {
            var userId = User.Identity.GetUserId();
            var beneficiaries = await db.Beneficiaries
                .Where(b => b.UserId == userId && b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return View(beneficiaries);
        }

        // GET: Banking/AddBeneficiary
        public ActionResult AddBeneficiary()
        {
            return View();
        }

        // POST: Banking/AddBeneficiary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddBeneficiary(Beneficiary beneficiary)
        {
            if (!ModelState.IsValid)
            {
                return View(beneficiary);
            }

            var userId = User.Identity.GetUserId();
            beneficiary.UserId = userId;
            beneficiary.DateAdded = DateTime.Now;
            beneficiary.IsActive = true;

            db.Beneficiaries.Add(beneficiary);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Beneficiary {beneficiary.Name} added successfully!";

            return RedirectToAction("Beneficiaries");
        }

        // POST: Banking/DeleteBeneficiary/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteBeneficiary(int id)
        {
            var userId = User.Identity.GetUserId();
            var beneficiary = await db.Beneficiaries
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (beneficiary == null)
            {
                return HttpNotFound();
            }

            beneficiary.IsActive = false;
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Beneficiary {beneficiary.Name} removed successfully!";

            return RedirectToAction("Beneficiaries");
        }

        // GET: Banking/ExportStatement/{accountId}
        public async Task<ActionResult> ExportStatement(int accountId, DateTime? fromDate, DateTime? toDate)
        {
            var userId = User.Identity.GetUserId();
            var account = await db.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null)
            {
                return HttpNotFound();
            }

            var query = db.Transactions
                .Where(t => t.AccountId == accountId);

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(t => t.TransactionDate < endDate);
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.Account = account;
            ViewBag.Transactions = transactions;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View();
        }

        // Helper method to generate account number
        private string GenerateAccountNumber()
        {
            var random = new Random();
            return "SB" + DateTime.Now.ToString("yyMMdd") + random.Next(100000, 999999).ToString();
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