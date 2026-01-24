using myapp.Models;
using System.Collections.Generic;

namespace myapp.ViewModels
{
    public class UserMenuViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Menu> Menus { get; set; } = new List<Menu>();

        // Holds the permissions for each user and menu
        public Dictionary<int, Dictionary<int, UserMenuPermission>> Permissions { get; set; } = new Dictionary<int, Dictionary<int, UserMenuPermission>>();

        // Receives the submitted permissions from the form
        public Dictionary<int, Dictionary<int, SubmittedPermission>>? SubmittedPermissions { get; set; }
    }

    public class SubmittedPermission
    {
        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
