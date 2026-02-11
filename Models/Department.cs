using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class Department : BaseEntity
    {
        [Required]
        [Display(Name = "ชื่อฝ่าย")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "Active";

        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
