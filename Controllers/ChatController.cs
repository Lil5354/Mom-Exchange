// Controllers/ChatController.cs
using B_M.Models;
using B_M.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace B_M.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserRepository userRepository;

        public ChatController()
        {
            db = new ApplicationDbContext();
            userRepository = new UserRepository();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
                userRepository?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Chat
        public ActionResult Index()
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy danh sách cuộc hội thoại
                var conversations = GetConversationSummaries(user.UserID);

                var viewModel = new ChatViewModel
                {
                    CurrentUserID = user.UserID,
                    CurrentUserName = user.UserDetails?.FullName ?? user.Email,
                    Conversations = conversations
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Chat/Conversation/{userId}
        public ActionResult Conversation(int userId)
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var otherUser = userRepository.GetUserById(userId);
                if (otherUser == null)
                {
                    return HttpNotFound();
                }

                // Lấy tin nhắn giữa 2 user
                var messages = db.Messages
                    .Where(m => (m.SenderID == currentUser.UserID && m.ReceiverID == userId) ||
                               (m.SenderID == userId && m.ReceiverID == currentUser.UserID))
                    .OrderBy(m => m.SentAt)
                    .ToList();

                // Đánh dấu tin nhắn từ otherUser là đã đọc
                var unreadMessages = messages.Where(m => m.ReceiverID == currentUser.UserID && !m.IsRead).ToList();
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                db.SaveChanges();

                var messageViewModels = messages.Select(m => new MessageViewModel
                {
                    MessageID = m.MessageID,
                    SenderID = m.SenderID,
                    ReceiverID = m.ReceiverID,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    SenderName = m.Sender.UserDetails?.FullName ?? m.Sender.Email,
                    SenderAvatar = m.Sender.UserDetails?.ProfilePictureURL ?? "/images/avatar-default.jpg",
                    IsFromCurrentUser = m.SenderID == currentUser.UserID
                }).ToList();

                var viewModel = new ConversationViewModel
                {
                    CurrentUserID = currentUser.UserID,
                    CurrentUserName = currentUser.UserDetails?.FullName ?? currentUser.Email,
                    CurrentUserAvatar = currentUser.UserDetails?.ProfilePictureURL ?? "/images/avatar-default.jpg",
                    OtherUserID = otherUser.UserID,
                    OtherUserName = otherUser.UserDetails?.FullName ?? otherUser.Email,
                    OtherUserAvatar = otherUser.UserDetails?.ProfilePictureURL ?? "/images/avatar-default.jpg",
                    Messages = messageViewModels,
                    IsOnline = true // Tạm thời hard-code
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Chat/GetConversationSummaries
        public ActionResult GetConversationSummaries()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." }, JsonRequestBehavior.AllowGet);
                }

                var conversations = GetConversationSummaries(currentUser.UserID);
                
                // Convert to anonymous objects for JSON serialization
                var conversationData = conversations?.Select(c => new {
                    userID = c.OtherUserID,
                    userName = c.OtherUserName,
                    userAvatar = c.OtherUserAvatar,
                    lastMessage = c.LastMessage,
                    unreadCount = c.UnreadCount,
                    isOnline = c.IsOnline
                }).Cast<object>().ToList() ?? new List<object>();
                
                return Json(new { success = true, conversations = conversationData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"GetConversationSummaries Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Chat/GetMessages
        public ActionResult GetMessages(int userId)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine($"GetMessages: Current user not found for identity: {userIdentity}");
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." }, JsonRequestBehavior.AllowGet);
                }

                System.Diagnostics.Debug.WriteLine($"GetMessages: CurrentUserID={currentUser.UserID}, TargetUserID={userId}");

                // Check if target user exists
                var targetUser = userRepository.GetUserById(userId);
                if (targetUser == null)
                {
                    System.Diagnostics.Debug.WriteLine($"GetMessages: Target user not found for ID: {userId}");
                    return Json(new { success = false, message = "Không tìm thấy người dùng đích." }, JsonRequestBehavior.AllowGet);
                }

                // Get messages using simpler approach
                var allMessages = db.Messages
                    .Where(m => (m.SenderID == currentUser.UserID && m.ReceiverID == userId) || 
                               (m.SenderID == userId && m.ReceiverID == currentUser.UserID))
                    .OrderBy(m => m.SentAt)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"GetMessages: Found {allMessages.Count} messages");

                var messages = allMessages?.Select(m => new {
                    messageID = m.MessageID,
                    content = m.Content,
                    isFromCurrentUser = m.SenderID == currentUser.UserID,
                    timeDisplay = m.SentAt.ToString("HH:mm")
                }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, messages = messages }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"GetMessages Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Chat/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(SendMessageViewModel model)
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                if (string.IsNullOrWhiteSpace(model.Content))
                {
                    return Json(new { success = false, message = "Nội dung tin nhắn không được để trống." });
                }

                var message = new Message
                {
                    SenderID = currentUser.UserID,
                    ReceiverID = model.ReceiverID,
                    Content = model.Content.Trim(),
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                db.Messages.Add(message);
                db.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = "Gửi tin nhắn thành công.",
                    messageId = message.MessageID,
                    sentAt = message.SentAt.ToString("HH:mm")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: Chat/GetMessages
        public ActionResult GetMessages(int otherUserId, long? lastMessageId = null)
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." }, JsonRequestBehavior.AllowGet);
                }

                var query = db.Messages
                    .Where(m => (m.SenderID == currentUser.UserID && m.ReceiverID == otherUserId) ||
                               (m.SenderID == otherUserId && m.ReceiverID == currentUser.UserID));

                if (lastMessageId.HasValue)
                {
                    query = query.Where(m => m.MessageID > lastMessageId.Value);
                }

                var messages = query
                    .OrderBy(m => m.SentAt)
                    .Take(50)
                    .ToList();

                // Đánh dấu tin nhắn từ otherUser là đã đọc
                var unreadMessages = messages.Where(m => m.ReceiverID == currentUser.UserID && !m.IsRead).ToList();
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                db.SaveChanges();

                var messageViewModels = messages.Select(m => new MessageViewModel
                {
                    MessageID = m.MessageID,
                    SenderID = m.SenderID,
                    ReceiverID = m.ReceiverID,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    SenderName = m.Sender.UserDetails?.FullName ?? m.Sender.Email,
                    SenderAvatar = m.Sender.UserDetails?.ProfilePictureURL ?? "/images/avatar-default.jpg",
                    IsFromCurrentUser = m.SenderID == currentUser.UserID
                }).ToList();

                return Json(new { 
                    success = true, 
                    messages = messageViewModels,
                    hasMore = messages.Count == 50
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Chat/GetUnreadCount
        public ActionResult GetUnreadCount()
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return Json(new { success = false, unreadCount = 0 }, JsonRequestBehavior.AllowGet);
                }

                var unreadCount = db.Messages
                    .Where(m => m.ReceiverID == currentUser.UserID && !m.IsRead)
                    .Count();

                return Json(new { success = true, unreadCount = unreadCount }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, unreadCount = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<ConversationSummary> GetConversationSummaries(int currentUserId)
        {
            // Lấy danh sách user đã có tin nhắn với currentUser
            var userIds = db.Messages
                .Where(m => m.SenderID == currentUserId || m.ReceiverID == currentUserId)
                .Select(m => m.SenderID == currentUserId ? m.ReceiverID : m.SenderID)
                .Distinct()
                .ToList();

            var conversations = new List<ConversationSummary>();

            foreach (var userId in userIds)
            {
                var otherUser = userRepository.GetUserById(userId);
                if (otherUser == null) continue;

                // Lấy tin nhắn cuối cùng
                var lastMessage = db.Messages
                    .Where(m => (m.SenderID == currentUserId && m.ReceiverID == userId) ||
                               (m.SenderID == userId && m.ReceiverID == currentUserId))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                // Đếm tin nhắn chưa đọc từ otherUser
                var unreadCount = db.Messages
                    .Where(m => m.SenderID == userId && m.ReceiverID == currentUserId && !m.IsRead)
                    .Count();

                conversations.Add(new ConversationSummary
                {
                    OtherUserID = userId,
                    OtherUserName = otherUser.UserDetails?.FullName ?? otherUser.Email,
                    OtherUserAvatar = !string.IsNullOrEmpty(otherUser.UserDetails?.ProfilePictureURL)
                        ? otherUser.UserDetails.ProfilePictureURL
                        : "/images/avatar-default.jpg",
                    LastMessage = lastMessage?.Content ?? "",
                    LastMessageTime = lastMessage?.SentAt ?? DateTime.MinValue,
                    UnreadCount = unreadCount,
                    IsOnline = true // Tạm thời hard-code
                });
            }

            return conversations.OrderByDescending(c => c.LastMessageTime).ToList();
        }

        // GET: Chat/GetMessagesSimple - Alternative method
        public ActionResult GetMessagesSimple(int userId)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                System.Diagnostics.Debug.WriteLine($"GetMessagesSimple: Identity={userIdentity}, TargetUserId={userId}");
                
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Current user not found" }, JsonRequestBehavior.AllowGet);
                }

                System.Diagnostics.Debug.WriteLine($"GetMessagesSimple: CurrentUserID={currentUser.UserID}");

                // Try to get all messages first
                var allMessages = db.Messages.ToList();
                System.Diagnostics.Debug.WriteLine($"GetMessagesSimple: Total messages in DB: {allMessages.Count}");

                // Filter messages
                var filteredMessages = allMessages.Where(m => 
                    (m.SenderID == currentUser.UserID && m.ReceiverID == userId) || 
                    (m.SenderID == userId && m.ReceiverID == currentUser.UserID))
                    .OrderBy(m => m.SentAt)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"GetMessagesSimple: Filtered messages: {filteredMessages.Count}");

                var messages = filteredMessages.Select(m => new {
                    messageID = m.MessageID,
                    content = m.Content,
                    isFromCurrentUser = m.SenderID == currentUser.UserID,
                    timeDisplay = m.SentAt.ToString("HH:mm")
                }).ToList();

                return Json(new { success = true, messages = messages }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMessagesSimple Error: {ex.Message}");
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Chat/TestMessages - Debug method
        public ActionResult TestMessages()
        {
            try
            {
                var messageCount = db.Messages.Count();
                var userCount = db.Users.Count();
                
                // Test specific user messages
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                var userMessages = currentUser != null ? db.Messages.Where(m => m.SenderID == currentUser.UserID || m.ReceiverID == currentUser.UserID).Count() : 0;
                
                return Json(new { 
                    success = true, 
                    messageCount = messageCount,
                    userCount = userCount,
                    currentUserId = currentUser?.UserID ?? 0,
                    userMessages = userMessages,
                    message = "Database connection OK" 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Database Error: " + ex.Message,
                    stackTrace = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
