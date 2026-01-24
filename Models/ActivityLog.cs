using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Display(Name = "วัน-เวลา")]
        public DateTime Timestamp { get; set; }

        [Display(Name = "ผู้ใช้งาน")]
        public string? UserName { get; set; }

        [Display(Name = "การกระทำ")]
        public string? Action { get; set; }

        [Display(Name = "รายละเอียด")]
        public string? Details { get; set; }

        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }
    }
}
