// File: Models/User.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class User
    {
        public int UserID { get; set; }

        [StringLength(50)]
        public string UserName { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // Role: 1 = Admin, 2 = Mom, 3 = Brand
        public byte Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        // === CỘT MỚI: THEO YÊU CẦU TẦNG SỮA MẸ ===
        public int MilkDonationStatus { get; set; } = 0;
        // 0: NotDonor (Chưa đăng ký cho tặng)
        // 1: BasicDeclared (Đã hoàn thành khai báo cơ bản - Tầng 1)
        // 2: PendingVerification (Đã nộp hồ sơ y tế, chờ Admin duyệt)
        // 3: HealthVerified (Đã xác thực y tế đầy đủ - Tầng 2)
        // 4: Rejected (Bị từ chối)

        // Google OAuth Integration
        [StringLength(255)]
        public string GoogleId { get; set; } // Google OAuth Subject ID

        // Navigation properties
        public UserDetails UserDetails { get; set; }
    }
}

