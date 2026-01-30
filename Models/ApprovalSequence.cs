using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class ApprovalSequence : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "ชื่อฝ่าย")]
        public string? Department { get; set; }

        public List<Approver> Approvers { get; set; } = new List<Approver>();

        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "Active";
    }
}
