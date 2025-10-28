// Models/ProductViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ProductCondition { get; set; }
        public string Location { get; set; }
        public string PostingType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAffiliateEnabled { get; set; }
        public decimal? AffiliateCommissionRate { get; set; }
        public string CategoryName { get; set; }
        public string SellerName { get; set; }
        public string SellerAvatar { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string PrimaryImageUrl { get; set; }

        // Computed properties
        public string FormattedPrice => PostingType == "Exchange" || PostingType == "Donate" ? PostingType == "Exchange" ? "Trao đổi" : "Miễn phí" : Price.ToString("N0") + " VND";
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "Pending": return "Chờ duyệt";
                    case "Approved": return "Đã duyệt";
                    case "Rejected": return "Bị từ chối";
                    case "Hidden": return "Đã ẩn";
                    default: return Status;
                }
            }
        }
        public string StatusClass
        {
            get
            {
                switch (Status)
                {
                    case "Pending": return "status-pending";
                    case "Approved": return "status-approved";
                    case "Rejected": return "status-rejected";
                    case "Hidden": return "status-hidden";
                    default: return "status-unknown";
                }
            }
        }
    }

    public class ProductDetailViewModel : ProductViewModel
    {
        public int Quantity { get; set; }
        public double SellerReputationScore { get; set; }
    }

    public class ProductIndexViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public int? CurrentCategoryId { get; set; }
        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
    }

    public class MyProductsViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [Display(Name = "Tên sản phẩm")]
        [StringLength(255)]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; }

        [Display(Name = "Giá bán")]
        public decimal? Price { get; set; }

        [Display(Name = "Tình trạng sản phẩm")]
        [StringLength(50)]
        public string ProductCondition { get; set; }

        [Display(Name = "Địa điểm")]
        [StringLength(255)]
        public string Location { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại đăng bài")]
        [Display(Name = "Loại đăng bài")]
        public string PostingType { get; set; } = "Sell";

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; } = 1;

        [Display(Name = "Bật Affiliate Marketing")]
        public bool IsAffiliateEnabled { get; set; }

        [Display(Name = "Tỷ lệ hoa hồng (%)")]
        [Range(0.1, 50, ErrorMessage = "Tỷ lệ hoa hồng từ 0.1% đến 50%")]
        public decimal? AffiliateCommissionRate { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();
    }

    
}

