using System;
using System.Linq;
using System.Web.Mvc;
using B_M.Models;

namespace B_M.Controllers
{
    public class NotificationApiController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        private int? GetCurrentUserId()
        {
            if (Session["UserID"] != null)
            {
                return (int)Session["UserID"];
            }

            // Fallback to identity-based lookup similar to NotificationController
            try
            {
                var userIdentity = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(userIdentity))
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == userIdentity || u.Email == userIdentity);
                    if (user != null) return user.UserID;
                }
            }
            catch { }
            return null;
        }

        // GET: /NotificationApi/GetUnreadCount
        [HttpGet]
        public ActionResult GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Json(new { success = true, count = 0 }, JsonRequestBehavior.AllowGet);
                }

                var count = db.Notifications.Count(n => n.UserID == userId.Value && !n.IsRead);
                return Json(new { success = true, count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, count = 0, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /NotificationApi/GetNotifications?page=0&size=10
        [HttpGet]
        public ActionResult GetNotifications(int page = 0, int size = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Json(new { success = true, data = new object[0] }, JsonRequestBehavior.AllowGet);
                }

                if (size <= 0) size = 10;
                if (page < 0) page = 0;

                var query = db.Notifications
                    .Where(n => n.UserID == userId.Value)
                    .OrderByDescending(n => n.CreatedAt);

                var items = query
                    .Skip(page * size)
                    .Take(size)
                    .ToList()
                    .Select(n => new
                    {
                        Id = n.NotificationID,
                        Title = n.Title,
                        Message = n.Message,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        Type = n.Type
                    });

                return Json(new { success = true, data = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = new object[0], error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /NotificationApi/MarkAsRead
        [HttpPost]
        public ActionResult MarkAsRead(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }

                var notif = db.Notifications.FirstOrDefault(n => n.NotificationID == id && n.UserID == userId.Value);
                if (notif != null)
                {
                    notif.IsRead = true;
                    db.SaveChanges();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /NotificationApi/MarkAllAsRead
        [HttpPost]
        public ActionResult MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }
                var notifs = db.Notifications.Where(n => n.UserID == userId.Value && !n.IsRead).ToList();
                foreach (var n in notifs)
                {
                    n.IsRead = true;
                }
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}


