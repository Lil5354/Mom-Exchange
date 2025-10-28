using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using B_M.Models;

namespace B_M.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRepository userRepository = new UserRepository();

        // GET: Notification
        public ActionResult Index()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var notifications = db.Notifications
                    .Where(n => n.UserID == currentUser.UserID)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .ToList();

                return View(notifications);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Notification/GetUnreadCount
        public ActionResult GetUnreadCount()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
                }

                var unreadCount = db.Notifications
                    .Count(n => n.UserID == currentUser.UserID && !n.IsRead);

                return Json(new { count = unreadCount }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Notification/GetRecentNotifications
        public ActionResult GetRecentNotifications()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập lại." }, JsonRequestBehavior.AllowGet);
                }

                var notifications = db.Notifications
                    .Where(n => n.UserID == currentUser.UserID)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .Select(n => new {
                        notificationID = n.NotificationID,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type,
                        isRead = n.IsRead,
                        createdAt = n.CreatedAt
                    })
                    .ToList();

                return Json(new { success = true, notifications = notifications }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Notification/MarkAsRead
        [HttpPost]
        public ActionResult MarkAsRead(long notificationId)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập lại." });
                }

                var notification = db.Notifications
                    .FirstOrDefault(n => n.NotificationID == notificationId && n.UserID == currentUser.UserID);

                if (notification != null)
                {
                    notification.IsRead = true;
                    db.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        public ActionResult MarkAllAsRead()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập lại." });
                }

                var unreadNotifications = db.Notifications
                    .Where(n => n.UserID == currentUser.UserID && !n.IsRead)
                    .ToList();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                db.SaveChanges();

                return Json(new { success = true, message = "Đã đánh dấu tất cả thông báo là đã đọc." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
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
}
