using System;
using System.Collections.Generic;
using B_M.Models;
using B_M.Repositories;

namespace B_M.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _notificationRepository;

        public NotificationService()
        {
            _notificationRepository = new NotificationRepository();
        }

        public List<Notification> GetUserNotifications(int userId)
        {
            return _notificationRepository.GetUserNotifications(userId);
        }

        public bool SendNotification(int userId, string title, string message, string type = "info")
        {
            try
            {
                // Implementation would create and save notification
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool MarkAsRead(int notificationId)
        {
            return _notificationRepository.MarkAsRead(notificationId);
        }
    }
}

