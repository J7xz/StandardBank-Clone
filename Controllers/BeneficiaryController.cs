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
    public class BeneficiaryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Beneficiary/Index
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var beneficiaries = db.Beneficiaries
                .Where(b => b.UserId == userId && b.IsActive)
                .OrderBy(b => b.Name)
                .ToList();

            return View(beneficiaries);
        }

        // GET: Beneficiary/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Beneficiary/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Beneficiary beneficiary)
        {
            if (!ModelState.IsValid)
            {
                return View(beneficiary);
            }

            var userId = User.Identity.GetUserId();

            // Check if beneficiary already exists for this user
            var existing = await db.Beneficiaries
                .FirstOrDefaultAsync(b => b.UserId == userId &&
                                         b.AccountNumber == beneficiary.AccountNumber &&
                                         b.IsActive);

            if (existing != null)
            {
                ModelState.AddModelError("", "This beneficiary already exists in your list.");
                return View(beneficiary);
            }

            beneficiary.UserId = userId;
            beneficiary.DateAdded = DateTime.Now;
            beneficiary.IsActive = true;

            db.Beneficiaries.Add(beneficiary);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Beneficiary '{beneficiary.Name}' added successfully!";

            return RedirectToAction("Index");
        }

        // GET: Beneficiary/Edit/{id}
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var beneficiary = await db.Beneficiaries
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);

            if (beneficiary == null)
            {
                return HttpNotFound();
            }

            return View(beneficiary);
        }

        // POST: Beneficiary/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Beneficiary beneficiary)
        {
            if (!ModelState.IsValid)
            {
                return View(beneficiary);
            }

            var userId = User.Identity.GetUserId();
            var existing = await db.Beneficiaries
                .FirstOrDefaultAsync(b => b.Id == beneficiary.Id && b.UserId == userId);

            if (existing == null)
            {
                return HttpNotFound();
            }

            // Update fields
            existing.Name = beneficiary.Name;
            existing.AccountNumber = beneficiary.AccountNumber;
            existing.BankName = beneficiary.BankName;
            existing.Nickname = beneficiary.Nickname;

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Beneficiary '{existing.Name}' updated successfully!";

            return RedirectToAction("Index");
        }

        // GET: Beneficiary/Delete/{id}
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var beneficiary = await db.Beneficiaries
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);

            if (beneficiary == null)
            {
                return HttpNotFound();
            }

            return View(beneficiary);
        }

        // POST: Beneficiary/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            var beneficiary = await db.Beneficiaries
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (beneficiary == null)
            {
                return HttpNotFound();
            }

            // Soft delete - just mark as inactive
            beneficiary.IsActive = false;
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Beneficiary '{beneficiary.Name}' removed successfully!";

            return RedirectToAction("Index");
        }

        // GET: Beneficiary/Details/{id}
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var beneficiary = await db.Beneficiaries
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);

            if (beneficiary == null)
            {
                return HttpNotFound();
            }

            return View(beneficiary);
        }

        // POST: Beneficiary/QuickAdd
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> QuickAdd(string name, string accountNumber, string bankName, string nickname)
        {
            var userId = User.Identity.GetUserId();

            try
            {
                var beneficiary = new Beneficiary
                {
                    Name = name,
                    AccountNumber = accountNumber,
                    BankName = bankName,
                    Nickname = nickname,
                    UserId = userId,
                    DateAdded = DateTime.Now,
                    IsActive = true
                };

                db.Beneficiaries.Add(beneficiary);
                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Beneficiary added successfully!", id = beneficiary.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to add beneficiary: " + ex.Message });
            }
        }

        // GET: Beneficiary/GetBeneficiaries
        public JsonResult GetBeneficiaries()
        {
            var userId = User.Identity.GetUserId();
            var beneficiaries = db.Beneficiaries
                .Where(b => b.UserId == userId && b.IsActive)
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    accountNumber = b.AccountNumber,
                    bankName = b.BankName,
                    nickname = b.Nickname
                })
                .OrderBy(b => b.name)
                .ToList();

            return Json(beneficiaries, JsonRequestBehavior.AllowGet);
        }

        // GET: Beneficiary/Search
        public JsonResult Search(string term)
        {
            var userId = User.Identity.GetUserId();
            var beneficiaries = db.Beneficiaries
                .Where(b => b.UserId == userId && b.IsActive &&
                           (b.Name.Contains(term) ||
                            b.AccountNumber.Contains(term) ||
                            b.Nickname.Contains(term) ||
                            b.BankName.Contains(term)))
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    accountNumber = b.AccountNumber,
                    bankName = b.BankName,
                    nickname = b.Nickname,
                    displayName = string.IsNullOrEmpty(b.Nickname) ? b.Name : $"{b.Nickname} ({b.Name})"
                })
                .Take(10)
                .ToList();

            return Json(beneficiaries, JsonRequestBehavior.AllowGet);
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