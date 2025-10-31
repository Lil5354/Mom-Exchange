using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models.Entities
{
    public class PayOSPaymentLink
    {
        [Key]
        public int Id { get; set; }
        
        // Link ID from PayOS
        [StringLength(50)]
        public string PayOSLinkId { get; set; }
        
        // Order Code
        [Required]
        [StringLength(20)]
        public string OrderCode { get; set; }
        
        // Checkout URL from PayOS
        [StringLength(500)]
        public string CheckoutUrl { get; set; }
        
        // QR Code text (VietQR format)
        [StringLength(2000)]
        public string QrCode { get; set; }
        
        // Payment status: 0 = Pending, 1 = Paid, 2 = Cancelled, 3 = Expired
        public byte Status { get; set; }
        
        // Amount to be paid
        [Required]
        public decimal Amount { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        
        public PayOSPaymentLink()
        {
            Status = 0; // Pending
            CreatedAt = DateTime.Now;
        }
    }
}

