using System.ComponentModel.DataAnnotations;
using myapp.Models;

namespace myapp.Models
{
    public class ApprovalSequence : BaseEntity
    {
        [Required]
        [Display(Name = "ชื่อฝ่าย")]
        public string? Department { get; set; }

        [Display(Name = "แผนก")]
        public string? Section { get; set; }
        
        [Required]
        [Display(Name = "ประเภทการแจ้งซ่อม")]
        public RequestType RequestType { get; set; }

        public List<Approver> Approvers { get; set; } = new List<Approver>();

        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "Active";
    }
}
