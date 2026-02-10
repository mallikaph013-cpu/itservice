using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class Approver : BaseEntity
    {
        public int ApprovalSequenceId { get; set; }
        [ForeignKey("ApprovalSequenceId")]
        public ApprovalSequence? ApprovalSequence { get; set; }

        [Required]
        public int UserId { get; set; } // Changed to int
        [ForeignKey("UserId")] // Add this line
        public User? User { get; set; }

        [Required]
        public int Order { get; set; }
    }
}
