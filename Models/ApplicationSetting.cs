using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    /// <summary>
    /// Model for storing application settings in database
    /// Supports different data types and encryption for sensitive data
    /// </summary>
    [Table("ApplicationSettings")]
    public class ApplicationSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Category of setting (Email, Security, Notification, System, Backup, Monitoring)
        /// </summary>
        [Required]
        [StringLength(50)]
        [Index("IX_Category_Key", 1, IsUnique = true)]
        public string Category { get; set; }

        /// <summary>
        /// Setting key name
        /// </summary>
        [Required]
        [StringLength(100)]
        [Index("IX_Category_Key", 2, IsUnique = true)]
        public string Key { get; set; }

        /// <summary>
        /// Setting value stored as string (can be parsed to appropriate type)
        /// </summary>
        [Column(TypeName = "nvarchar(MAX)")]
        public string Value { get; set; }

        /// <summary>
        /// Data type of the value (String, Int, Bool, DateTime, JSON)
        /// </summary>
        [StringLength(20)]
        public string DataType { get; set; }

        /// <summary>
        /// Description of what this setting does
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Whether this value is encrypted in database
        /// </summary>
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// When this setting was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// User ID who last updated this setting (nullable, no FK constraint for safety)
        /// </summary>
        public int? UpdatedBy { get; set; }

        public ApplicationSetting()
        {
            LastUpdated = DateTime.Now;
            DataType = "String";
            IsEncrypted = false;
        }
    }
}

