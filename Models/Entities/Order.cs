using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        
        // Order Code - unique payment code for transaction
        [Required]
        [StringLength(20)]
        public string OrderCode { get; set; } // Format: ME-YYYYMMDD-XXXXX
        
        // Customer information
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }
        
        // Brand information (which brand this order belongs to)
        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }
        
        // Order status
        // 0 = Pending, 1 = Paid, 2 = Processing, 3 = Shipped, 4 = Delivered, 5 = Cancelled
        public byte Status { get; set; }
        
        // Shipping information
        [Required]
        [StringLength(200)]
        public string ShippingName { get; set; }
        
        [Required]
        [StringLength(15)]
        public string ShippingPhone { get; set; }
        
        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; }
        
        // Financial information
        public decimal SubTotal { get; set; }      // Total before commission
        public decimal Commission { get; set; }     // Marketplace commission (e.g., 5%)
        public decimal TotalAmount { get; set; }   // Final amount to be paid
        
        // Order dates
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        
        // Additional info
        [StringLength(500)]
        public string Note { get; set; } // Customer notes
        
        [StringLength(100)]
        public string PaymentMethod { get; set; } // "Bank Transfer", "Cash on Delivery", etc.
        
        // Navigation property
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        
        public Order()
        {
            OrderItems = new List<OrderItem>();
            Status = 0; // Pending
            CreatedAt = DateTime.Now;
        }
    }
}

