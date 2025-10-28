// Models/MilkDonationPost.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class MilkDonationPost
    {
        public long PostID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int VerificationTier { get; set; }

        // 1: Open (Đang mở), 2: Closed (Đã đóng)
        public int Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        // Computed properties for UI
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case 1: return "Đang mở";
                    case 2: return "Đã đóng";
                    default: return "Không xác định";
                }
            }
        }

        public string VerificationTierText
        {
            get
            {
                switch (VerificationTier)
                {
                    case 1: return "Tầng 1 - Khai báo cơ bản";
                    case 3: return "Tầng 2 - Đã xác thực y tế";
                    default: return "Chưa xác minh";
                }
            }
        }

        public string VerificationTierClass
        {
            get
            {
                switch (VerificationTier)
                {
                    case 1: return "tier-basic";
                    case 3: return "tier-verified";
                    default: return "tier-none";
                }
            }
        }
    }
}