using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class Department : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "ชื่อฝ่าย")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "Active";
    }
}
