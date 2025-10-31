// Models/AdminPostC2CViewModel.cs
using System;
using System.Collections.Generic;

namespace B_M.Models
{
    public class AdminPostC2CListViewModel
    {
        public List<AdminPostC2CItemViewModel> Posts { get; set; } = new List<AdminPostC2CItemViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalPosts { get; set; }
        public int? StatusFilter { get; set; }
        public int? CategoryFilter { get; set; }
        public int? ListingTypeFilter { get; set; }
        
        // Statistics
        public int ActivePosts { get; set; }
        public int SoldPosts { get; set; }
        public int SalePosts { get; set; }
        public int ExchangePosts { get; set; }
    }

    public class AdminPostC2CItemViewModel
    {
        public long PostID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserAvatarUrl { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string Condition { get; set; }
        public decimal? Price { get; set; }
        public int ListingType { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ImageCount { get; set; }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case 1: return "Đang mở";
                    case 2: return "Đã bán/đổi";
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

        public string ListingTypeText
        {
            get
            {
                switch (ListingType)
                {
                    case 1: return "Chỉ bán";
                    case 2: return "Chỉ trao đổi";
                    case 3: return "Bán hoặc trao đổi";
                    default: return "Không xác định";
                }
            }
        }

        public string ListingTypeClass
        {
            get
            {
                switch (ListingType)
                {
                    case 1: return "badge-primary";
                    case 2: return "badge-info";
                    case 3: return "badge-warning";
                    default: return "badge-secondary";
                }
            }
        }

        public string PriceText
        {
            get
            {
                if (Price.HasValue)
                    return Price.Value.ToString("N0") + " VNĐ";
                return "Trao đổi";
            }
        }
    }

    public class AdminPostC2CDetailViewModel : AdminPostC2CItemViewModel
    {
        public string Content { get; set; }
        public List<PostC2CImageViewModel> Images { get; set; } = new List<PostC2CImageViewModel>();
    }

    public class PostC2CImageViewModel
    {
        public long ImageID { get; set; }
        public string ImageUrl { get; set; }
        public string FileName { get; set; }
    }
}
