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
    [Authorize(Policy = "IsITSupport")]
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

            return View("~/Views/MyTasks/Index.cshtml", assignedTasks);
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

            if (currentUser == null) 
            {
                return Unauthorized();
            }

            // If the task has no responsible user, assign the current user.
            if (supportRequest.ResponsibleUserId == null) 
            {
                supportRequest.ResponsibleUserId = currentUser.Id;
            }
            // Ensure the current user is the responsible one.
            else if (supportRequest.ResponsibleUserId != currentUser.Id)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null) return NotFound();

            ModelState.Clear(); // Clear model state to avoid validation on other properties

            supportRequest.Status = SupportRequestStatus.Rejected;
            supportRequest.RevisionReason = reason; // Or a specific field for rejection reason
            supportRequest.UpdatedAt = DateTime.UtcNow;
            supportRequest.UpdatedBy = User.Identity.Name;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ปฏิเสธงานเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(int id)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null) return NotFound();

            ModelState.Clear();

            supportRequest.Status = SupportRequestStatus.Done;
            supportRequest.UpdatedAt = DateTime.UtcNow;
            supportRequest.UpdatedBy = User.Identity.Name;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ดำเนินการเสร็จสิ้นเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnForRevision(int id, string revisionReason)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null)
            {
                return NotFound();
            }

            ModelState.Clear();

            var employeeId = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);

            if (currentUser == null || supportRequest.ResponsibleUserId != currentUser.Id)
            {
                return Forbid();
            }

            supportRequest.Status = SupportRequestStatus.RevisionNeeded;
            supportRequest.RevisionReason = revisionReason;
            supportRequest.UpdatedAt = DateTime.UtcNow;
            supportRequest.UpdatedBy = User.Identity?.Name ?? "system";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "ส่งกลับไปแก้ไขเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Index));
        }
    }
}
