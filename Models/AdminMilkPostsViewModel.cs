// Models/AdminMilkPostsViewModel.cs
using System;
using System.Collections.Generic;

namespace B_M.Models
{
    public class AdminMilkPostsViewModel
    {
        public List<AdminMilkPostItemViewModel> Posts { get; set; } = new List<AdminMilkPostItemViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalPosts { get; set; }
        public int? StatusFilter { get; set; }
    }

    public class AdminMilkPostItemViewModel
    {
        public long PostID { get; set; }
        public int UserID { get; set; }
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public string DonorAvatarUrl { get; set; }
        public string Location { get; set; }
        public DateTime DateOfExpression { get; set; }
        public string DietInfo { get; set; }
        public string StorageInfo { get; set; }
        public int VerificationTier { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }

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
                    case 1: return "badge-success";
                    case 2: return "badge-secondary";
                    default: return "badge-warning";
                }
            }
        }

        public string VerificationTierText
        {
            get
            {
                switch (VerificationTier)
                {
                    case 1: return "Tầng 1";
                    case 3: return "Tầng 2";
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

    public class AdminMilkPostDetailViewModel : AdminMilkPostItemViewModel
    {
        public string Note { get; set; }
    }
}
