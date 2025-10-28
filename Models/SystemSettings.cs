using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class SystemSettings
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; }
        
        [StringLength(1000)]
        public string SettingValue { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SettingType { get; set; } // "email", "security", "notification", "system", "backup", "monitoring"
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public DateTime LastModified { get; set; }
        
        [StringLength(100)]
        public string ModifiedBy { get; set; }
        
        public bool IsActive { get; set; }
    }
}




