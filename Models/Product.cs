
// Models/Product.cs
using System.Collections.Generic;

namespace B_M.Models // Namespace của bạn có thể khác
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Price { get; set; }
        public string ShortDescription { get; set; }
        public string DetailedDescription { get; set; }
        public string Condition { get; set; }
        public string Brand { get; set; }
        public string Location { get; set; }
        public string SellerName { get; set; }
        public string SellerAvatarUrl { get; set; }
        public double SellerRating { get; set; }
        public int SellerReviewCount { get; set; }
        public List<string> ImageUrls { get; set; }
    }
}