using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Create
        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = new SelectList(await _context.Departments.Where(d => d.Status == "Active").ToListAsync(), "Name", "Name");
            ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.Status == "Active").ToListAsync(), "Name", "Name");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("EmployeeId,FirstName,LastName,Password,Department,Section,Role,IsITStaff,IsDxStaff,CanApprove")] User user)
        {
            if (user.IsITStaff)
            {
                user.Department = "IT";
                user.Role = "ITSupport";
            }

            if (ModelState.IsValid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(await _context.Departments.Where(d => d.Status == "Active").ToListAsync(), "Name", "Name", user.Department);
            ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.Status == "Active").ToListAsync(), "Name", "Name", user.Section);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.Departments = new SelectList(await _context.Departments.Where(d => d.Status == "Active").ToListAsync(), "Name", "Name", user.Department);
            ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.Status == "Active").ToListAsync(), "Name", "Name", user.Section);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,FirstName,LastName,Password,Department,Section,Role,IsITStaff,IsDxStaff,CanApprove")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                ModelState.Remove("Password");
            }

            if (user.IsITStaff)
            {
                user.Department = "IT";
                user.Role = "ITSupport";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = await _context.Users.FindAsync(id);
                    if (userToUpdate == null)
                    {
                        return NotFound();
                    }

                    userToUpdate.EmployeeId = user.EmployeeId;
                    userToUpdate.FirstName = user.FirstName;
                    userToUpdate.LastName = user.LastName;
                    userToUpdate.Department = user.Department;
                    userToUpdate.Section = user.Section;
                    userToUpdate.Role = user.Role;
                    userToUpdate.IsITStaff = user.IsITStaff;
                    userToUpdate.IsDxStaff = user.IsDxStaff;
                    userToUpdate.CanApprove = user.CanApprove;

                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        userToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(user.Password); 
                    }


                    _context.Update(userToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(await _context.Departments.Where(d => d.Status == "Active").ToListAsync(), "Name", "Name", user.Department);
            ViewBag.Sections = new SelectList(await _context.Sections.Where(s => s.Status == "Active").ToListAsync(), "Name", "Name", user.Section);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}