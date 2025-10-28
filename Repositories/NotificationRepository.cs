using System;
using System.Collections.Generic;
using System.Linq;
using B_M.Models;

namespace B_M.Repositories
{
    public class NotificationRepository : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository()
        {
            _context = new ApplicationDbContext();
        }

        public List<Notification> GetUserNotifications(int userId)
        {
            return _context.Notifications
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public bool MarkAsRead(int notificationId)
        {
            try
            {
                var notification = _context.Notifications.Find(notificationId);
                if (notification == null) return false;

                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

