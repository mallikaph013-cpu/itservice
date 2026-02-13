using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using myapp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Authorize(Policy = "IsApprover")]
    public class ApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApprovalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(SupportRequestStatus? selectedStatus)
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(employeeId))
            {
                return Unauthorized("Employee ID claim not found.");
            }

            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            var viewModel = new ApprovalIndexViewModel();
            
            var statuses = Enum.GetValues(typeof(SupportRequestStatus)).Cast<SupportRequestStatus>();
            viewModel.StatusOptions = new SelectList(statuses, selectedStatus);
            viewModel.SelectedStatus = selectedStatus ?? SupportRequestStatus.Pending; // Default to Pending

            List<SupportRequest> requests;

            if (viewModel.SelectedStatus == SupportRequestStatus.Pending)
            {
                var userApproverIds = await _context.Approvers
                    .Where(a => a.UserId == currentUser.Id)
                    .Select(a => a.Id)
                    .ToListAsync();

                var firstApprovalDepts = await _context.ApprovalSequences
                    .Where(s => s.Approvers.Any(a => a.Order == 1 && a.UserId == currentUser.Id))
                    .Select(s => s.Department)
                    .ToListAsync();

                requests = await _context.SupportRequests
                    .Include(r => r.CurrentApprover).ThenInclude(a => a!.User)
                    .Where(r => r.Status == SupportRequestStatus.Pending && !string.IsNullOrEmpty(r.Department) &&
                        (
                            (r.CurrentApproverId != null && userApproverIds.Contains(r.CurrentApproverId.Value)) ||
                            (r.CurrentApproverId == null && firstApprovalDepts.Contains(r.Department))
                        ))
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            else // For Approved/Rejected, show historical requests
            {
                var userManagedDepts = await _context.ApprovalSequences
                    .Where(s => s.Approvers.Any(a => a.UserId == currentUser.Id))
                    .Select(s => s.Department)
                    .Distinct()
                    .ToListAsync();

                requests = await _context.SupportRequests
                    .Include(r => r.CurrentApprover).ThenInclude(a => a!.User)
                    .Where(r => r.Status == viewModel.SelectedStatus && userManagedDepts.Contains(r.Department))
                    .OrderByDescending(r => r.UpdatedAt)
                    .ToListAsync();
            }

            viewModel.Requests = requests;
            return View(viewModel);
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Approve(int id)
{
    var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(employeeId))
    {
        return Json(new { success = false, message = "User claim not found." });
    }

    var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
    if (currentUser == null)
    {
        return Json(new { success = false, message = "User not found." });
    }

    var supportRequest = await _context.SupportRequests
        .Include(r => r.CurrentApprover) // Keep include for potential logging or other uses
        .FirstOrDefaultAsync(r => r.Id == id);

    if (supportRequest == null || string.IsNullOrEmpty(supportRequest.Department))
    {
        return Json(new { success = false, message = "ไม่พบรายการที่ต้องการ" });
    }

    var sequence = await _context.ApprovalSequences
        .Include(s => s.Approvers.OrderBy(a => a.Order))
        .FirstOrDefaultAsync(s => s.Department == supportRequest.Department);

    if (sequence == null)
    {
        return Json(new { success = false, message = "ไม่พบสายอนุมัติสำหรับแผนกนี้" });
    }

    // Determine the acting user's order in the sequence.
    var actingApprover = sequence.Approvers.FirstOrDefault(a => a.UserId == currentUser.Id);

    // If the user is not a designated approver for this department, they can't approve.
    if (actingApprover == null)
    {
        return Json(new { success = false, message = "คุณไม่มีสิทธิ์อนุมัติรายการนี้" });
    }
    
    // Check if it's the user's turn to approve.
    // It's their turn if the request is unassigned and they are the first approver,
    // OR if the request is assigned to them.
    bool isFirstApproverAction = supportRequest.CurrentApproverId == null && actingApprover.Order == 1;
    bool isAssignedApproverAction = supportRequest.CurrentApproverId == actingApprover.Id;

    if (!isFirstApproverAction && !isAssignedApproverAction)
    {
        return Json(new { success = false, message = "ไม่ใช่ตาของคุณในการอนุมัติรายการนี้" });
    }

    var currentOrder = actingApprover.Order;
    var nextApprover = sequence.Approvers.FirstOrDefault(a => a.Order > currentOrder);

    if (nextApprover == null)
    {
        // Final approval step
        bool isITRequest = supportRequest.Department == "IT";
        if (isITRequest)
        {
            supportRequest.Status = SupportRequestStatus.InProgress;
        }
        else
        {
            supportRequest.Status = SupportRequestStatus.Approved;
        }
        supportRequest.CurrentApproverId = null;
    }
    else
    {
        // Intermediate approval step, pass to the next approver.
        supportRequest.Status = SupportRequestStatus.Pending;
        supportRequest.CurrentApproverId = nextApprover.Id;
    }

    supportRequest.UpdatedAt = DateTime.UtcNow;
    supportRequest.UpdatedBy = User.FindFirstValue(ClaimTypes.Name);
    await _context.SaveChangesAsync();

    return Json(new { success = true, newStatus = supportRequest.Status.ToString() });
}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null)
            {
                return Json(new { success = false, message = "ไม่พบรายการที่ต้องการ" });
            }

            supportRequest.Status = SupportRequestStatus.Rejected;
            supportRequest.CurrentApproverId = null; 
            supportRequest.UpdatedAt = DateTime.UtcNow;
            supportRequest.UpdatedBy = User.FindFirstValue(ClaimTypes.Name);

            await _context.SaveChangesAsync();

            return Json(new { success = true, newStatus = supportRequest.Status.ToString() });
        }
    }
}
