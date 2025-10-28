// Models/Order.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class Order
    {
        public long OrderID { get; set; }

        [Required]
        public int BuyerUserID { get; set; }

        [Required]
        public int BrandID { get; set; } // Required - 1 đơn hàng chỉ của 1 Brand

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        // 1: Pending, 2: Paid, 3: Shipped, 4: Completed, 5: Cancelled
        public int OrderStatus { get; set; } = 1;

        // Navigation properties
        [ForeignKey("BuyerUserID")]
        public virtual User BuyerUser { get; set; }

        [ForeignKey("BrandID")]
        public virtual Brand Brand { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}

