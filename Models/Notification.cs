using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class Notification
    {
        [Key]
        public long NotificationID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public int Type { get; set; }
        // 1: Request Received (Người cho nhận được yêu cầu)
        // 2: Request Accepted (Người nhận được chấp nhận)
        // 3: Request Declined (Người nhận bị từ chối)
        // 4: Post Closed (Bài đăng đã đóng)

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }

        // Related data (optional)
        public long? RelatedPostID { get; set; }
        public long? RelatedRequestID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        // Computed properties
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case 1: return "Yêu cầu mới";
                    case 2: return "Yêu cầu được chấp nhận";
                    case 3: return "Yêu cầu bị từ chối";
                    case 4: return "Bài đăng đã đóng";
                    default: return "Thông báo";
                }
            }
        }

        public string TypeClass
        {
            get
            {
                switch (Type)
                {
                    case 1: return "info";
                    case 2: return "success";
                    case 3: return "warning";
                    case 4: return "secondary";
                    default: return "info";
                }
            }
        }

        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                if (timeSpan.TotalMinutes < 1)
                    return "Vừa xong";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} phút trước";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} giờ trước";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} ngày trước";
                return CreatedAt.ToString("dd/MM/yyyy");
            }
        }
    }
}
