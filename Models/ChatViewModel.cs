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
        
        // Product context information
        public List<ProductContext> RelatedProducts { get; set; } = new List<ProductContext>();
        
        public string TimeDisplay => GetTimeDisplay();
        
        private string GetTimeDisplay()
        {
            var timeSpan = DateTime.Now - LastMessageTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            
            return LastMessageTime.ToString("dd/MM/yyyy");
        }
    }

    public class ProductContext
    {
        public long PostID { get; set; }
        public string PostTitle { get; set; }
        public string PostType { get; set; } // "milk-donation", "c2c"
        public string PostTypeDisplay { get; set; } // "Cho tặng sữa mẹ", "Thanh lý", "Trao đổi"
        public decimal? Price { get; set; }
        public int? ListingType { get; set; } // For C2C: 1=Sell, 2=Exchange, 3=Both
        public string ImageUrl { get; set; }
        public DateTime PostDate { get; set; }
        public int Status { get; set; } // 1=Open, 2=Closed
        
        public string StatusText
        {
            get
            {
                return Status == 1 ? "Đang mở" : "Đã đóng";
            }
        }
        
        public string PriceDisplay
        {
            get
            {
                if (PostType == "milk-donation")
                    return "Miễn phí";
                
                if (ListingType == 2)
                    return "Trao đổi";
                
                if (Price.HasValue)
                    return string.Format("{0:N0} đ", Price.Value);
                
                return "Trao đổi";
            }
        }
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
        public List<ProductContext> RelatedProducts { get; set; } = new List<ProductContext>();
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