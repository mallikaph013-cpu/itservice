using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class ApprovalHistory : BaseEntity
    {
        [ForeignKey("SupportRequest")]
        public int SupportRequestId { get; set; }
        public SupportRequest? SupportRequest { get; set; }
        public string? ApproverName { get; set; }
        public bool Approved { get; set; }
        public string? Comments { get; set; }
        public DateTime ApprovalDate { get; set; }
    }
}
