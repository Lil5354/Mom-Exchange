// Models/ChatViewModel.cs
using System;
using System.Collections.Generic;

namespace B_M.Models
{
    public class ChatViewModel
    {
        public int CurrentUserID { get; set; }
        public string CurrentUserName { get; set; }
        public List<ConversationSummary> Conversations { get; set; } = new List<ConversationSummary>();
    }

    public class ConversationSummary
    {
        public int OtherUserID { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserAvatar { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ConversationViewModel
    {
        public int CurrentUserID { get; set; }
        public string CurrentUserName { get; set; }
        public string CurrentUserAvatar { get; set; }
        
        public int OtherUserID { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserAvatar { get; set; }
        
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        public bool IsOnline { get; set; }
    }

    public class MessageViewModel
    {
        public long MessageID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        
        public bool IsFromCurrentUser { get; set; }
        public string TimeDisplay => SentAt.ToString("HH:mm");
        public string DateDisplay => SentAt.ToString("dd/MM/yyyy");
    }

    public class SendMessageViewModel
    {
        public int ReceiverID { get; set; }
        public string Content { get; set; }
    }
}
