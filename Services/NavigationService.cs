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
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal?.Identity?.IsAuthenticated != true)
            {
                return new List<Menu>(); // Return empty list if not authenticated
            }

            // Fetch all menus with their submenus for structural reference
            var allMenus = await _context.Menus
                .Include(m => m.SubMenus)
                .Where(m => m.ParentMenuId == null) // Start with top-level menus
                .ToListAsync();

            // If the user is an Admin, return all menus without filtering
            if (userPrincipal.IsInRole("Admin"))
            {
                return allMenus;
            }

            // For non-admin users, filter based on permissions.
            var employeeId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(employeeId))
            {
                return new List<Menu>();
            }

            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
            if (currentUser == null)
            {
                return new List<Menu>();
            }

            var allowedMenuIds = await _context.UserMenuPermissions
                .Where(p => p.UserId == currentUser.Id && p.CanRead)
                .Select(p => p.MenuId)
                .ToListAsync();
            
            var canApprove = userPrincipal.HasClaim("CanApprove", "True");

            return FilterMenus(allMenus, allowedMenuIds, canApprove);
        }

        private List<Menu> FilterMenus(List<Menu> menus, List<int> allowedMenuIds, bool canApprove)
        {
            var accessibleMenus = new List<Menu>();

            foreach (var menu in menus)
            {
                // Specific check for the Approval menu
                if (menu.ControllerName == "Approval" && !canApprove)
                {
                    continue; // Skip this menu if the user is not an approver
                }

                var isDirectlyAllowed = allowedMenuIds.Contains(menu.Id);
                List<Menu> accessibleSubMenus = new List<Menu>();

                if (menu.SubMenus != null && menu.SubMenus.Any())
                {
                    // Recursively filter submenus
                    accessibleSubMenus = FilterMenus(menu.SubMenus.ToList(), allowedMenuIds, canApprove);
                    // Update submenus with the filtered list
                    menu.SubMenus = accessibleSubMenus;
                }

                var hasAccessibleChildren = accessibleSubMenus.Any();

                if (isDirectlyAllowed || hasAccessibleChildren)
                {
                    accessibleMenus.Add(menu);
                }
            }

            return accessibleMenus;
        }
    }
}
