using System.ComponentModel.DataAnnotations;

namespace myapp.Models
{
    public class UserMenuPermission : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int MenuId { get; set; }
        public Menu? Menu { get; set; }

        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
