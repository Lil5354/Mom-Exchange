// Models/AdminMilkDonationViewModels.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace B_M.Models
{
    public class AdminMilkPostsViewModel
    {
        public List<AdminMilkPostItemViewModel> Posts { get; set; } = new List<AdminMilkPostItemViewModel>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalPosts { get; set; }
        public string StatusFilter { get; set; } = "all";

        // Statistics
        public int ActivePosts => Posts.Count(p => p.Status == 1); // Open posts
        public int ClosedPosts => Posts.Count(p => p.Status == 2); // Closed posts
    }

    public class AdminMilkPostItemViewModel
    {
        public long PostID { get; set; }
        public int UserID { get; set; }
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public string DonorAvatarUrl { get; set; }
        public string Location { get; set; }
        public DateTime? DateOfExpression { get; set; }
        public string DietInfo { get; set; }
        public string StorageInfo { get; set; }
        public int VerificationTier { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties
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

        public string StatusClass
        {
            get
            {
                switch (Status)
                {
                    case 1: return "status-open";
                    case 2: return "status-closed";
                    default: return "status-unknown";
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

    public class AdminMilkPostDetailViewModel
    {
        public long PostID { get; set; }
        public int UserID { get; set; }
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public string DonorPhone { get; set; }
        public string DonorAvatarUrl { get; set; }
        public string Location { get; set; }
        public DateTime? DateOfExpression { get; set; }
        public string DietInfo { get; set; }
        public string StorageInfo { get; set; }
        public string Note { get; set; }
        public int VerificationTier { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FullContent { get; set; }

        // User details
        public int MilkDonationStatus { get; set; }
        public DateTime UserCreatedAt { get; set; }
        public bool IsUserActive { get; set; }

        // Computed properties
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

        public string StatusClass
        {
            get
            {
                switch (Status)
                {
                    case 1: return "status-open";
                    case 2: return "status-closed";
                    default: return "status-unknown";
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

        public string MilkDonationStatusText
        {
            get
            {
                switch (MilkDonationStatus)
                {
                    case 0: return "Không phải người hiến sữa";
                    case 1: return "Tầng 1 - Khai báo cơ bản";
                    case 2: return "Chờ xác thực y tế";
                    case 3: return "Tầng 2 - Đã xác thực y tế";
                    case 4: return "Bị từ chối xác thực";
                    default: return "Không xác định";
                }
            }
        }

        public string MilkDonationStatusClass
        {
            get
            {
                switch (MilkDonationStatus)
                {
                    case 0: return "status-none";
                    case 1: return "status-basic";
                    case 2: return "status-pending";
                    case 3: return "status-verified";
                    case 4: return "status-rejected";
                    default: return "status-unknown";
                }
            }
        }
    }
}
