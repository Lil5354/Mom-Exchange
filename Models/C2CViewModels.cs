// Models/C2CViewModels.cs - View models for the C2C module
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace B_M.Models
{
    public class C2CPostCardViewModel
    {
        public long PostID { get; set; }
        public string Title { get; set; }
        public string ContentSnippet { get; set; }
        public string Condition { get; set; }
        public int ListingType { get; set; }
        public decimal? Price { get; set; }
        public string Location { get; set; }
        public string PrimaryImageUrl { get; set; }
        public int SellerUserID { get; set; }
        public string SellerName { get; set; }
        public string SellerAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<string> ExchangeCategoryNames { get; set; }

        public string ListingTypeText
        {
            get
            {
                switch (ListingType)
                {
                    case 1: return "Chỉ bán";
                    case 2: return "Chỉ trao đổi";
                    case 3: return "Bán hoặc trao đổi";
                }
                return "Không xác định";
            }
        }
    }

    public class C2CCreateViewModel
    {
        [Required]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Condition { get; set; }

        [Required]
        public int ListingType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? Price { get; set; }

        // Exchange preferences
        public List<int> ExchangeCategoryIDs { get; set; } = new List<int>();

        // Images
        public List<string> ImageUrls { get; set; } = new List<string>();
        public HttpPostedFileBase[] ImageFiles { get; set; }
    }
}


