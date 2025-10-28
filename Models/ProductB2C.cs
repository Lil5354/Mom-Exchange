// Models/ProductB2C.cs - B2C Products for Module 3
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class ProductB2C
    {
        [Key]
        public long ProductID { get; set; }

        [Required]
        public int BrandID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(255)]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; } = 0;

        // Affiliate settings
        [Required]
        public bool IsAffiliateEnabled { get; set; } = false;

        public decimal? AffiliateCommissionRate { get; set; }

        // Navigation properties
        [ForeignKey("BrandID")]
        public virtual Brand Brand { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

        public virtual ICollection<ProductB2CImage> Images { get; set; } = new List<ProductB2CImage>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<AffiliateClick> AffiliateClicks { get; set; } = new List<AffiliateClick>();
    }
}
