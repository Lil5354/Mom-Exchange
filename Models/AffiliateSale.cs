// Models/AffiliateSale.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class AffiliateSale
    {
        public long AffiliateSaleID { get; set; }

        [Required]
        public long OrderID { get; set; }

        [Required]
        public int AffiliatorUserID { get; set; }

        [Required]
        public int BuyerUserID { get; set; }

        public decimal OrderTotalAmount { get; set; }

        public decimal CommissionAmount { get; set; }

        // 1: Pending, 2: Approved, 3: Paid, 4: Cancelled
        public int Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("AffiliatorUserID")]
        public virtual User AffiliatorUser { get; set; }

        [ForeignKey("BuyerUserID")]
        public virtual User BuyerUser { get; set; }
    }
}

