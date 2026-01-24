using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; } // e.g., "หน้าหลัก", "ข่าวสาร", "ข้อมูลฝ่าย"

        [StringLength(50)]
        public string? ControllerName { get; set; }

        [StringLength(50)]
        public string? ActionName { get; set; }

        public bool IsDropdown { get; set; } = false;

        // For child menus in a dropdown
        public int? ParentMenuId { get; set; }
        public virtual Menu? ParentMenu { get; set; }
        public virtual ICollection<Menu> SubMenus { get; set; } = new List<Menu>();

        public virtual ICollection<UserMenuPermission> UserMenuPermissions { get; set; } = new List<UserMenuPermission>();
    }
}
