using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class MilkDonationRequest
    {
        public long RequestID { get; set; }

        [Required]
        public long PostID { get; set; }

        [Required]
        public int RecipientUserID { get; set; } // Người Nhận

        [Required]
        public int DonorUserID { get; set; } // Người Cho

        [Required]
        public int Status { get; set; } = 0;
        // 0: Pending (Đang chờ)
        // 1: Accepted (Đã chấp nhận)
        // 2: Declined (Đã từ chối)
        // 3: Closed (Tin đăng đã đóng)

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string Note { get; set; }

        // Navigation properties
        [ForeignKey("PostID")]
        public virtual MilkDonationPost Post { get; set; }

        [ForeignKey("RecipientUserID")]
        public virtual User RecipientUser { get; set; }

        [ForeignKey("DonorUserID")]
        public virtual User DonorUser { get; set; }

        // Computed properties for UI
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case 0: return "Đang chờ";
                    case 1: return "Đã chấp nhận";
                    case 2: return "Đã từ chối";
                    case 3: return "Tin đăng đã đóng";
                    default: return "Không xác định";
                }
            }
        }

        public string StatusClass
        {
            get
            {
                switch (Status)
                {
                    case 0: return "status-pending";
                    case 1: return "status-accepted";
                    case 2: return "status-declined";
                    case 3: return "status-closed";
                    default: return "status-unknown";
                }
            }
        }
    }
}
