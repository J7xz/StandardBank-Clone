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
    public class NotificationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Notification/Index
        public async Task<ActionResult> Index(int page = 1, int pageSize = 20)
        {
            var userId = User.Identity.GetUserId();

            var query = db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DateSent);

            var totalNotifications = await query.CountAsync();
            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new NotificationViewModel
            {
                Notifications = notifications,
                UnreadCount = await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead),
                TotalNotifications = totalNotifications
            };

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalNotifications / pageSize);

            return View(model);
        }

        // GET: Notification/Unread
        public async Task<ActionResult> Unread()
        {
            var userId = User.Identity.GetUserId();

            var notifications = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.DateSent)
                .ToListAsync();

            var model = new NotificationViewModel
            {
                Notifications = notifications,
                UnreadCount = notifications.Count,
                TotalNotifications = await db.Notifications.CountAsync(n => n.UserId == userId)
            };

            return View("Index", model);
        }

        // GET: Notification/Details/{id}
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            var notification = await db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return HttpNotFound();
            }

            // Mark as read when viewed
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.DateRead = DateTime.Now;
                await db.SaveChangesAsync();
            }

            return View(notification);
        }

        // POST: Notification/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MarkAsRead(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var notification = await db.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (notification != null && !notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.DateRead = DateTime.Now;
                    await db.SaveChangesAsync();

                    return Json(new { success = true, message = "Notification marked as read." });
                }

                return Json(new { success = false, message = "Notification not found or already read." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MarkAllAsRead()
        {
            try
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

                return Json(new { success = true, message = "All notifications marked as read." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Notification/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var notification = await db.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (notification != null)
                {
                    db.Notifications.Remove(notification);
                    await db.SaveChangesAsync();

                    return Json(new { success = true, message = "Notification deleted." });
                }

                return Json(new { success = false, message = "Notification not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Notification/DeleteAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteAll()
        {
            try
            {
                var userId = User.Identity.GetUserId();

                var notifications = await db.Notifications
                    .Where(n => n.UserId == userId && n.IsRead)
                    .ToListAsync();

                if (notifications.Any())
                {
                    db.Notifications.RemoveRange(notifications);
                    await db.SaveChangesAsync();
                }

                return Json(new { success = true, message = "All read notifications deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Notification/DeleteAllUnread
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteAllUnread()
        {
            try
            {
                var userId = User.Identity.GetUserId();

                var notifications = await db.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                if (notifications.Any())
                {
                    db.Notifications.RemoveRange(notifications);
                    await db.SaveChangesAsync();
                }

                return Json(new { success = true, message = "All unread notifications deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Notification/Count
        public async Task<JsonResult> Count()
        {
            var userId = User.Identity.GetUserId();

            var unreadCount = await db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            var totalCount = await db.Notifications
                .CountAsync(n => n.UserId == userId);

            return Json(new { unread = unreadCount, total = totalCount }, JsonRequestBehavior.AllowGet);
        }

        // GET: Notification/GetLatest
        public async Task<JsonResult> GetLatest(int count = 5)
        {
            var userId = User.Identity.GetUserId();

            var notifications = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.DateSent)
                .Take(count)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.LinkUrl,
                    n.NotificationType,
                    Date = n.DateSent.ToString("dd MMM yyyy HH:mm"),
                    TimeAgo = GetTimeAgo(n.DateSent)
                })
                .ToListAsync();

            return Json(notifications, JsonRequestBehavior.AllowGet);
        }

        // GET: Notification/GetAllLatest
        public async Task<JsonResult> GetAllLatest(int count = 10)
        {
            var userId = User.Identity.GetUserId();

            var notifications = await db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DateSent)
                .Take(count)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.LinkUrl,
                    n.NotificationType,
                    Date = n.DateSent.ToString("dd MMM yyyy HH:mm"),
                    TimeAgo = GetTimeAgo(n.DateSent),
                    IsRead = n.IsRead
                })
                .ToListAsync();

            return Json(notifications, JsonRequestBehavior.AllowGet);
        }

        // GET: Notification/GetUnreadCount
        public async Task<JsonResult> GetUnreadCount()
        {
            var userId = User.Identity.GetUserId();

            var count = await db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Json(new { count }, JsonRequestBehavior.AllowGet);
        }

        // GET: Notification/GetTypes
        public JsonResult GetTypes()
        {
            var types = new List<string> { "All", "InApp", "Email" };
            return Json(types, JsonRequestBehavior.AllowGet);
        }

        // GET: Notification/Filter
        public async Task<ActionResult> Filter(string type, string searchTerm, int page = 1, int pageSize = 20)
        {
            var userId = User.Identity.GetUserId();

            var query = db.Notifications
                .Where(n => n.UserId == userId);

            // Filter by type
            if (!string.IsNullOrEmpty(type) && type != "All")
            {
                query = query.Where(n => n.NotificationType == type);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(n => n.Title.Contains(searchTerm) ||
                                         n.Message.Contains(searchTerm));
            }

            var totalNotifications = await query.CountAsync();
            var notifications = await query
                .OrderByDescending(n => n.DateSent)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new NotificationViewModel
            {
                Notifications = notifications,
                UnreadCount = await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead),
                TotalNotifications = totalNotifications
            };

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalNotifications / pageSize);
            ViewBag.FilterType = type;
            ViewBag.SearchTerm = searchTerm;

            return View("Index", model);
        }

        // POST: Notification/Create (Admin only - for sending notifications)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> Create(string title, string message, string userId, string linkUrl = null)
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    NotificationType = "InApp",
                    DateSent = DateTime.Now,
                    IsRead = false,
                    UserId = userId,
                    LinkUrl = linkUrl
                };

                db.Notifications.Add(notification);
                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Notification sent successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to send notification: " + ex.Message });
            }
        }

        // GET: Notification/CreateBroadcast (Admin only)
        [Authorize(Roles = "Admin")]
        public ActionResult CreateBroadcast()
        {
            return View();
        }

        // POST: Notification/CreateBroadcast (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateBroadcast(string title, string message, string linkUrl = null)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = "Title and message are required.";
                return View();
            }

            var users = await db.Users.ToListAsync();

            foreach (var user in users)
            {
                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    NotificationType = "InApp",
                    DateSent = DateTime.Now,
                    IsRead = false,
                    UserId = user.Id,
                    LinkUrl = linkUrl
                };

                db.Notifications.Add(notification);
            }

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Broadcast sent to {users.Count} users successfully!";

            return RedirectToAction("Index", "Admin");
        }

        // Helper method to calculate time ago
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return "just now";

            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute(s) ago";

            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour(s) ago";

            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day(s) ago";

            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week(s) ago";

            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month(s) ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year(s) ago";
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