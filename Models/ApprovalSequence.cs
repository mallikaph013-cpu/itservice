using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class ApprovalSequence
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "ชื่อฝ่าย")]
        public string? Department { get; set; }

        public List<Approver> Approvers { get; set; } = new List<Approver>();

        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "Active";

        [Display(Name = "วันที่สร้าง")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "วันที่แก้ไข")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "ผู้สร้าง")]
        public string? CreatedBy { get; set; }

        [Display(Name = "ผู้แก้ไข")]
        public string? UpdatedBy { get; set; }
    }
}
