// Models/Product.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace B_M.Models.Entities // Namespace của bạn có thể khác
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Price { get; set; }
        public int StockQuantity { get; set; }
        public string ShortDescription { get; set; }
        public string DetailedDescription { get; set; }
        public string Condition { get; set; }
        
        // Foreign Key
        public int BrandId { get; set; }
        
        // Navigation property
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }
        
        public string Location { get; set; }
        public string SellerName { get; set; }
        public string SellerAvatarUrl { get; set; }
        public double SellerRating { get; set; }
        public int SellerReviewCount { get; set; }
        
        // Product status
        public bool IsActive { get; set; } = true;
        
        // Navigation property for images
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        
        // Helper property for backward compatibility (not mapped to database)
        [NotMapped]
        public List<string> ImageUrls 
        { 
            get 
            { 
                return ProductImages?.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).ToList() ?? new List<string>(); 
            }
            set 
            { 
                if (ProductImages == null) ProductImages = new List<ProductImage>();
                ProductImages.Clear();
                if (value != null)
                {
                    for (int i = 0; i < value.Count; i++)
                    {
                        ProductImages.Add(new ProductImage { ImageUrl = value[i], SortOrder = i, ProductId = this.Id });
                    }
                }
            }
        }
    }
}