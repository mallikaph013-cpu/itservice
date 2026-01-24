using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace myapp.Services
{
    public class NavigationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NavigationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Menu>> GetMenuItemsAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return new List<Menu>(); // Return empty list if not authenticated
            }

            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return new List<Menu>(); // Return empty list if user id is not found or invalid
            }

            // Fetch all menus with their submenus
            var menus = await _context.Menus
                .Include(m => m.SubMenus)
                .Where(m => m.ParentMenuId == null) // Start with top-level menus
                .ToListAsync();

            // If the user is an Admin, return all menus without filtering
            if (user.IsInRole("Admin"))
            {
                return menus;
            }

            // For other users, get the menu IDs assigned to the user where CanRead is true
            var allowedMenuIds = await _context.UserMenuPermissions
                .Where(p => p.UserId == userId && p.CanRead)
                .Select(p => p.MenuId)
                .ToListAsync();
            
            // Filter menus based on permissions
            return FilterMenus(menus, allowedMenuIds);
        }

        private List<Menu> FilterMenus(List<Menu> menus, List<int> allowedMenuIds)
        {
            var accessibleMenus = new List<Menu>();

            foreach (var menu in menus)
            {
                // A menu is accessible if it's directly in the allowed list
                var isDirectlyAllowed = allowedMenuIds.Contains(menu.Id);

                if (menu.SubMenus != null && menu.SubMenus.Any())
                {
                    // Recursively filter submenus
                    menu.SubMenus = FilterMenus(menu.SubMenus.ToList(), allowedMenuIds);
                }

                // A dropdown parent is accessible if any of its children are accessible
                var hasAccessibleChildren = menu.SubMenus != null && menu.SubMenus.Any();

                if (isDirectlyAllowed || hasAccessibleChildren)
                {
                    accessibleMenus.Add(menu);
                }
            }

            return accessibleMenus;
        }
    }
}
