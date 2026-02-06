using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "รหัสพนักงาน")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "ชื่อจริง")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "นามสกุล")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "แผนก")]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Display(Name = "ตำแหน่ง")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "เป็นเจ้าหน้าที่ IT")]
        public bool IsITStaff { get; set; } 

        [Display(Name = "ระบุว่า DX Approve")]
        public bool IsDxStaff { get; set; }
        
        [Display(Name = "สามารถอนุมัติได้")]
        public bool CanApprove { get; set; } 

        public string FullName => $"{FirstName} {LastName}";
    }
}
