// Models/OrderDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class OrderDetail
    {
        public long OrderDetailID { get; set; }

        [Required]
        public long OrderID { get; set; }

        [Required]
        public long ProductID { get; set; } // FK to Products_B2C

        [Required]
        public int Quantity { get; set; }

        public decimal PriceAtPurchase { get; set; } // Giá tại thời điểm mua

        // Navigation properties
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductID")]
        public virtual ProductB2C Product { get; set; }
    }
}

