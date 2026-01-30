using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace myapp.Controllers
{
    [Authorize]
    public class MyTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MyTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MyTasks
        public async Task<IActionResult> Index()
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return Unauthorized("User is not authenticated.");
            }
            
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
            if (currentUser == null)
            {
                return NotFound("Current user not found in the database.");
            }

            var assignedTasks = await _context.SupportRequests
                .Where(r => r.ResponsibleUserId == currentUser.Id && 
                           (r.Status == SupportRequestStatus.Approved || r.Status == SupportRequestStatus.InProgress))
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();

            return View(assignedTasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null)
            {
                return NotFound();
            }

            var employeeId = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);

            if (supportRequest.ResponsibleUserId != currentUser?.Id)
            {
                return Forbid(); // User is not the responsible person for this task
            }

            if (Enum.TryParse<SupportRequestStatus>(status, out var newStatus))
            {
                supportRequest.Status = newStatus;
                supportRequest.UpdatedAt = DateTime.UtcNow;
                supportRequest.UpdatedBy = User.Identity?.Name ?? "system";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"อัปเดตสถานะงานเป็น '{newStatus.ToString()}' เรียบร้อยแล้ว";
            }
            else
            {
                TempData["ErrorMessage"] = "สถานะที่ระบุไม่ถูกต้อง";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
