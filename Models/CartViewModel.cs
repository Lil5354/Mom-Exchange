// Models/CartViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace B_M.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string SellerName { get; set; }
        public int SellerID { get; set; }
        public string Location { get; set; }
        public int MaxQuantity { get; set; } = 1; // Available stock

        public decimal SubTotal => Price * Quantity;

        public bool IsAffiliateEnabled { get; set; }
        public decimal? AffiliateCommissionRate { get; set; }
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        public decimal TotalAmount => Items.Sum(item => item.SubTotal);
        
        public int TotalItems => Items.Sum(item => item.Quantity);
        
        public bool IsEmpty => !Items.Any();

        // Group items by seller for organized checkout
        public Dictionary<int, List<CartItem>> ItemsBySeller
        {
            get
            {
                return Items.GroupBy(item => item.SellerID)
                           .ToDictionary(g => g.Key, g => g.ToList());
            }
        }

        // Calculate total affiliate commission if applicable
        public decimal TotalAffiliateCommission
        {
            get
            {
                return Items.Where(item => item.IsAffiliateEnabled && item.AffiliateCommissionRate.HasValue)
                           .Sum(item => item.SubTotal * (item.AffiliateCommissionRate.Value / 100));
            }
        }
    }

    public class AddToCartViewModel
    {
        [Required]
        public int ProductID { get; set; }

        [Required]
        [Range(1, 999, ErrorMessage = "Số lượng phải từ 1 đến 999")]
        public int Quantity { get; set; } = 1;

        // Optional affiliate referrer tracking
        public int? AffiliateReferrerID { get; set; }
    }

    public class UpdateCartItemViewModel
    {
        [Required]
        public int ProductID { get; set; }

        [Required]
        [Range(0, 999, ErrorMessage = "Số lượng phải từ 0 đến 999")]
        public int Quantity { get; set; }
    }

    public class CartSummaryViewModel
    {
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        
        // For AJAX cart updates
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}

