using System;
using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class WorkItem : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public WorkItemStatus Status { get; set; } = WorkItemStatus.New;
    }
}
