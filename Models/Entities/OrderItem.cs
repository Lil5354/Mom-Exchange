using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        // Foreign key to Order
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        
        // Foreign key to Product (optional - products may be deleted)
        public int? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        
        // Product snapshot (in case product is deleted or changed)
        [StringLength(200)]
        public string ProductName { get; set; }
        
        [StringLength(100)]
        public string ProductPrice { get; set; }
        
        [StringLength(500)]
        public string ProductImageUrl { get; set; }
        
        // Quantity and pricing
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Store as decimal for calculations
        public decimal TotalPrice { get; set; }
        
        // Additional info
        public DateTime CreatedAt { get; set; }
        
        public OrderItem()
        {
            CreatedAt = DateTime.Now;
        }
    }
}

