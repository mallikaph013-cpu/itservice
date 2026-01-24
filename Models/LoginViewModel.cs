using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกรหัสพนักงาน")]
        [Display(Name = "รหัสพนักงาน")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "จดจำฉันไว้")]
        public bool RememberMe { get; set; }
    }
}
