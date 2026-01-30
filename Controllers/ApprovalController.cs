using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using myapp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using myapp.ViewModels; 

namespace myapp.Controllers
{
    [Authorize]
    public class ApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApprovalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return Unauthorized("Employee ID claim not found.");
            }

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            var requestsToApprove = await _context.SupportRequests
                .Include(r => r.CurrentApprover)
                    .ThenInclude(ca => ca != null ? ca.User : null)
                .Where(r => r.Status == SupportRequestStatus.Pending && r.CurrentApprover != null && r.CurrentApprover.UserId == currentUser.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Fix: Fetch all users into memory first, then sort by the calculated FullName property.
            var allUsers = await _context.Users.ToListAsync();
            var assignableUsers = allUsers.OrderBy(u => u.FullName).ToList();

            var viewModel = new ApprovalIndexViewModel
            {
                PendingRequests = requestsToApprove,
                AssignableUsers = assignableUsers
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int responsibleUserId)
        {
            var supportRequest = await _context.SupportRequests
                                                .Include(r => r.CurrentApprover)
                                                    .ThenInclude(ca => ca != null ? ca.ApprovalSequence : null)
                                                        .ThenInclude(s => s != null ? s.Approvers : null)
                                                .FirstOrDefaultAsync(r => r.Id == id);

            if (supportRequest == null)
            {
                return NotFound();
            }
            
            var userToAssign = await _context.Users.FindAsync(responsibleUserId);
            if (userToAssign == null)
            {
                ModelState.AddModelError(string.Empty, "ผู้ใช้ที่เลือกสำหรับมอบหมายงานไม่ถูกต้อง");
                return RedirectToAction(nameof(Index)); 
            }

            var currentApprover = supportRequest.CurrentApprover;
            bool isFinalApproval = false;

            if (currentApprover != null && currentApprover.ApprovalSequence?.Approvers != null)
            {
                var nextApprover = currentApprover.ApprovalSequence.Approvers
                                                  .OrderBy(a => a.Order)
                                                  .FirstOrDefault(a => a.Order > currentApprover.Order);

                if (nextApprover != null)
                {
                    supportRequest.CurrentApproverId = nextApprover.Id;
                }
                else
                {
                    isFinalApproval = true;
                }
            }
            else
            {
                isFinalApproval = true;
            }

            if(isFinalApproval)
            {
                supportRequest.Status = SupportRequestStatus.Approved;
                supportRequest.CurrentApproverId = null;
                supportRequest.ResponsibleUserId = responsibleUserId;
            }

            supportRequest.UpdatedAt = DateTime.UtcNow;
            supportRequest.UpdatedBy = User.Identity?.Name ?? "system";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = isFinalApproval 
                ? $"อนุมัติและมอบหมายงานให้ {userToAssign.FullName} เรียบร้อยแล้ว"
                : "อนุมัติรายการแจ้งซ่อมเรียบร้อยแล้ว และส่งต่อไปยังผู้อนุมัติคนถัดไป";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null)
            {
                return NotFound();
            }

            supportRequest.Status = SupportRequestStatus.Rejected;
            supportRequest.UpdatedAt = DateTime.UtcNow;
            supportRequest.UpdatedBy = User.Identity?.Name ?? "system";

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ปฏิเสธรายการแจ้งซ่อมเรียบร้อยแล้ว";

            return RedirectToAction(nameof(Index));
        }
    }
}
