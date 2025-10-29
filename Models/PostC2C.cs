// Models/PostC2C.cs - C2C Posts for Module 3
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class PostC2C
    {
        [Key]
        public long PostID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(100)]
        public string Condition { get; set; } // Ví dụ: "Mới 90%"

        public decimal? Price { get; set; } // Giá mong muốn

        [Required]
        public int ListingType { get; set; } = 1;
        // 1: Chỉ Bán (Price không NULL)
        // 2: Chỉ Trao đổi
        // 3: Cả Bán hoặc Trao đổi

        [Required]
        public int Status { get; set; } = 1; // 1: Open, 2: Sold/Exchanged

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

        public virtual ICollection<PostC2CImage> Images { get; set; } = new List<PostC2CImage>();
        public virtual ICollection<PostC2CExchangePreference> ExchangePreferences { get; set; } = new List<PostC2CExchangePreference>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

        // Computed properties for UI
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case 1: return "Đang mở";
                    case 2: return "Đã bán/trao đổi";
                    default: return "Không xác định";
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
    }
}
