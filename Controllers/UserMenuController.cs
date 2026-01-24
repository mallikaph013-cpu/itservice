using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserMenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserMenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserMenu
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            var menus = await _context.Menus.Include(m => m.SubMenus).Where(m => m.ParentMenuId == null).ToListAsync();
            var permissions = await _context.UserMenuPermissions.ToListAsync();

            var viewModel = new UserMenuViewModel
            {
                Users = users,
                Menus = menus,
                Permissions = new Dictionary<int, Dictionary<int, UserMenuPermission>>()
            };

            foreach (var user in users)
            {
                viewModel.Permissions[user.Id] = permissions
                    .Where(p => p.UserId == user.Id)
                    .ToDictionary(p => p.MenuId);
            }

            return View(viewModel);
        }

        // POST: UserMenu/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserMenuViewModel model)
        {
            // First, remove all existing permissions to avoid conflicts
            _context.UserMenuPermissions.RemoveRange(_context.UserMenuPermissions);
            await _context.SaveChangesAsync();

            // Now, add the new permissions from the form submission
            if (model.SubmittedPermissions != null)
            {
                var newPermissions = new List<UserMenuPermission>();
                foreach (var userEntry in model.SubmittedPermissions)
                {
                    var userId = userEntry.Key;
                    foreach (var menuEntry in userEntry.Value)
                    {
                        var menuId = menuEntry.Key;
                        var permission = menuEntry.Value;

                        // Add a permission only if at least one checkbox is checked
                        if (permission.CanRead || permission.CanCreate || permission.CanEdit || permission.CanDelete)
                        {
                            newPermissions.Add(new UserMenuPermission
                            {
                                UserId = userId,
                                MenuId = menuId,
                                CanRead = permission.CanRead,
                                CanCreate = permission.CanCreate,
                                CanEdit = permission.CanEdit,
                                CanDelete = permission.CanDelete
                            });
                        }
                    }
                }
                await _context.UserMenuPermissions.AddRangeAsync(newPermissions);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "อัปเดตสิทธิ์การเข้าถึงเมนูเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Index));
        }
    }
}
