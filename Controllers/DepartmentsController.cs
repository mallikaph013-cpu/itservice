using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Authorize]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var viewModel = new DepartmentSectionViewModel
            {
                Departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync(),
                Sections = await _context.Sections.Include(s => s.Department).OrderBy(s => s.Name).ToListAsync()
            };
            return View(viewModel);
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Status")] Department department)
        {
            if (ModelState.IsValid)
            {
                if (!await _context.Departments.AnyAsync(d => d.Name == department.Name))
                {
                    department.CreatedAt = System.DateTime.UtcNow;
                    department.UpdatedAt = System.DateTime.UtcNow;
                    department.CreatedBy = User.Identity?.Name ?? "system";
                    department.UpdatedBy = User.Identity?.Name ?? "system";
                    _context.Add(department);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Status,CreatedBy,CreatedAt")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var departmentToUpdate = await _context.Departments.FindAsync(id);
                    if(departmentToUpdate == null) return NotFound();

                    departmentToUpdate.Name = department.Name;
                    departmentToUpdate.Status = department.Status;
                    departmentToUpdate.UpdatedAt = System.DateTime.UtcNow;
                    departmentToUpdate.UpdatedBy = User.Identity?.Name ?? "system";

                    _context.Update(departmentToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
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
            return View(department);
        }


        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}
