using Microsoft.AspNetCore.Mvc;
using myapp.Models;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using myapp.Data; 
using Microsoft.EntityFrameworkCore; 
using System;
using myapp.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace myapp.Controllers
{
    [Authorize]
    public class ITSupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ITSupportController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var supportRequestsQuery = _context.SupportRequests
                                                .Include(r => r.CurrentApprover)
                                                .ThenInclude(a => a != null ? a.User : null)
                                                .AsQueryable();

            var isPrivilegedUser = User.IsInRole("Admin") || User.IsInRole("ITSupport");
            if (!isPrivilegedUser)
            {
                var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(employeeId))
                {
                    supportRequestsQuery = supportRequestsQuery.Where(r => r.EmployeeId == employeeId);
                }
                else
                {
                    return View(new List<SupportRequest>()); 
                }
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                var upperSearchString = searchString.ToUpper();
                supportRequestsQuery = supportRequestsQuery.Where(s => s.RequestType.ToString().ToUpper().Contains(upperSearchString) || 
                                                              s.ProblemDescription.ToUpper().Contains(upperSearchString) || 
                                                              s.RequesterName.ToUpper().Contains(upperSearchString));
            }

            var supportRequests = await supportRequestsQuery.OrderByDescending(r => r.CreatedAt).ToListAsync();
            ViewData["CurrentFilter"] = searchString;
            return View(supportRequests);
        }

        [Authorize(Policy = "CanAccessWorkQueue")]
        public async Task<IActionResult> WorkQueue(string searchString, SupportRequestStatus? selectedStatus)
        {   
            var statusToFilter = selectedStatus;

            var supportRequestsQuery = _context.SupportRequests
                                                .Include(r => r.CurrentApprover)
                                                .ThenInclude(a => a != null ? a.User : null)
                                                .Include(r => r.ResponsibleUser)
                                                .AsQueryable();

            if (statusToFilter.HasValue)
            {
                 supportRequestsQuery = supportRequestsQuery.Where(r => r.Status == statusToFilter.Value);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                var upperSearchString = searchString.ToUpper();
                supportRequestsQuery = supportRequestsQuery.Where(s => s.RequestType.ToString().ToUpper().Contains(upperSearchString) || 
                                                              s.ProblemDescription.ToUpper().Contains(upperSearchString) || 
                                                              s.RequesterName.ToUpper().Contains(upperSearchString));
            }

            var supportRequests = await supportRequestsQuery.OrderByDescending(r => r.CreatedAt).ToListAsync();

            var assignableUsers = await _context.Users
                                          .Where(u => u.Role == "ITSupport")
                                          .ToListAsync();

            var filterableStatuses = new List<SupportRequestStatus>
            {
                SupportRequestStatus.Approved,
                SupportRequestStatus.InProgress,
                SupportRequestStatus.Done,
                SupportRequestStatus.Pending,
                SupportRequestStatus.Rejected
            };

            var viewModel = new WorkQueueViewModel
            {
                SupportRequests = supportRequests,
                AssignableUsers = assignableUsers,
                StatusOptions = new SelectList(filterableStatuses, selectedStatus),
                SelectedStatus = selectedStatus
            };
            
            ViewData["CurrentFilter"] = searchString;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanAccessWorkQueue")]
        public async Task<IActionResult> AssignResponsibleUser(int supportRequestId, int userId)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(supportRequestId);
            var user = await _context.Users.FindAsync(userId);

            if (supportRequest == null || user == null)
            {
                return NotFound();
            }

            supportRequest.ResponsibleUserId = userId;
            supportRequest.Status = SupportRequestStatus.InProgress;
            supportRequest.UpdatedAt = DateTime.UtcNow;
            supportRequest.UpdatedBy = User.Identity?.Name ?? "system";

            _context.Update(supportRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"มอบหมายงานให้ {user.FullName} เรียบร้อยแล้ว";

            return RedirectToAction(nameof(WorkQueue));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanAccessWorkQueue")]
        public async Task<IActionResult> UpdateStatus(int id, SupportRequestStatus status)
        {
            var request = await _context.SupportRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            request.UpdatedAt = DateTime.UtcNow;
            request.UpdatedBy = User.Identity?.Name ?? "system";

            _context.Update(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "อัปเดตสถานะเรียบร้อยแล้ว";

            return RedirectToAction(nameof(WorkQueue));
        }

        public async Task<IActionResult> Create()
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);

            var supportRequest = new SupportRequest();

            if (currentUser != null)
            {
                supportRequest.EmployeeId = currentUser.EmployeeId;
                supportRequest.RequesterName = currentUser.FullName;
                supportRequest.Department = currentUser.Department;
            }
            
            return View(supportRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupportRequest supportRequest)
        {
            if (string.IsNullOrEmpty(supportRequest.EmployeeId))
            {
                ModelState.AddModelError("EmployeeId", "Employee ID is required.");
            }
            if (string.IsNullOrEmpty(supportRequest.RequesterName))
            {
                ModelState.AddModelError("RequesterName", "Requester Name is required.");
            }
            if (string.IsNullOrEmpty(supportRequest.Department))
            {
                ModelState.AddModelError("Department", "Department is required.");
            }

            if (ModelState.IsValid)
            {
                // Set the status to Pending and CurrentApproverId to null.
                // The ApprovalController is responsible for determining who should see this new request.
                supportRequest.Status = SupportRequestStatus.Pending;
                supportRequest.CurrentApproverId = null;

                supportRequest.CreatedAt = DateTime.UtcNow;
                supportRequest.UpdatedAt = DateTime.UtcNow;
                supportRequest.CreatedBy = User.Identity?.Name ?? "system";
                supportRequest.UpdatedBy = User.Identity?.Name ?? "system";

                _context.SupportRequests.Add(supportRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "บันทึกรายการแจ้งซ่อมสำเร็จแล้ว เอกสารจะถูกส่งไปตามลำดับการอนุมัติ";

                return RedirectToAction(nameof(Index));
            }
            
            return View(supportRequest);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.SupportRequests
                .Include(r => r.CurrentApprover)
                .ThenInclude(a => a != null ? a.User : null)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }
            return View(request);
        }

        // GET: ITSupport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null)
            {
                return NotFound();
            }
            
            if (supportRequest.Status != SupportRequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "ไม่สามารถแก้ไขรายการที่อนุมัติแล้วได้";
                return RedirectToAction(nameof(Index));
            }

            return View(supportRequest);
        }

        // POST: ITSupport/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupportRequest supportRequest)
        {
            if (id != supportRequest.Id)
            {
                return NotFound();
            }

            var requestToUpdate = await _context.SupportRequests.FindAsync(id);

            if (requestToUpdate == null)
            {
                return NotFound();
            }

            if (requestToUpdate.Status != SupportRequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "ไม่สามารถแก้ไขรายการที่อนุมัติแล้วได้";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                // Manually update the properties of the tracked entity
                requestToUpdate.EmployeeId = supportRequest.EmployeeId;
                requestToUpdate.RequesterName = supportRequest.RequesterName;
                requestToUpdate.Department = supportRequest.Department;
                requestToUpdate.RequestType = supportRequest.RequestType;
                requestToUpdate.Program = supportRequest.Program;
                requestToUpdate.SAPProblem = supportRequest.SAPProblem;
                requestToUpdate.ProblemDescription = supportRequest.ProblemDescription;
                requestToUpdate.Urgency = supportRequest.Urgency;
                // Copy other editable fields as necessary, omitting fields that shouldn't be changed.

                requestToUpdate.UpdatedAt = DateTime.UtcNow;
                requestToUpdate.UpdatedBy = User.Identity?.Name ?? "system";

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SupportRequests.Any(e => e.Id == supportRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "แก้ไขรายการแจ้งซ่อมสำเร็จแล้ว";
                return RedirectToAction(nameof(Index));
            }
            // If model state is invalid, return the view with the data entered by the user.
            return View(supportRequest);
        }

        // GET: ITSupport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supportRequest = await _context.SupportRequests
                .Include(s => s.CurrentApprover)
                .ThenInclude(a => a != null ? a.User : null)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supportRequest == null)
            {
                return NotFound();
            }

            if (supportRequest.Status != SupportRequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "ไม่สามารถลบรายการที่อนุมัติแล้วได้";
                return RedirectToAction(nameof(Index));
            }

            return View(supportRequest);
        }

        // POST: ITSupport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(id);
             if (supportRequest == null) // Add null check
            {
                return NotFound();
            }

            if (supportRequest.Status != SupportRequestStatus.Pending)
            {
                 TempData["ErrorMessage"] = "ไม่สามารถลบรายการที่อนุมัติแล้วได้";
                return RedirectToAction(nameof(Index));
            }
            _context.SupportRequests.Remove(supportRequest);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ลบรายการแจ้งซ่อมสำเร็จแล้ว";
            return RedirectToAction(nameof(Index));
        }
    }
}
