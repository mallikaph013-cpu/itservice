using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Section
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "ชื่อแผนก")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "Active";

        [Required]
        [Display(Name = "ฝ่าย")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
    }
}
