using System;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public abstract class BaseEntity
    {
        [Required]
        [Display(Name = "วันที่สร้าง")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "ผู้สร้าง")]
        public string? CreatedBy { get; set; }

        [Required]
        [Display(Name = "วันที่แก้ไข")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "ผู้แก้ไข")]
        public string? UpdatedBy { get; set; }
    }
}
