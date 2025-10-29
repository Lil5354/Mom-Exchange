// Models/Brand.cs - Restructured for Module 3
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace B_M.Models
{
    public class Brand
    {
        [Key]
        public int BrandID { get; set; }

        [Required]
        [StringLength(255)]
        public string BrandName { get; set; }

        [StringLength(1024)]
        public string LogoUrl { get; set; }

        public string Description { get; set; }
    }
}