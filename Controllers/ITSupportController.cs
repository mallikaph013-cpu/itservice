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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace myapp.Controllers
{
    [Authorize] // Require login for all actions in this controller
    public class ITSupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ITSupportController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // Helper method to check if user is authorized for a specific request
        private async Task<bool> IsAuthorized(int supportRequestId)
        {
            var isPrivilegedUser = User.IsInRole("Admin") || User.IsInRole("ITSupport");
            if (isPrivilegedUser) return true;

            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.SupportRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == supportRequestId);

            return request?.EmployeeId == employeeId;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var supportRequestsQuery = _context.SupportRequests
                                                .Include(s => s.CurrentApprover.User)
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
                    // If for some reason a logged-in user has no EmployeeId, return an empty list
                    return View(new List<SupportRequest>()); 
                }
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                supportRequestsQuery = supportRequestsQuery.Where(s => 
                    EF.Functions.Like(s.RequesterName, $"%{searchString}%") ||
                    EF.Functions.Like(s.ProblemDescription, $"%{searchString}%") ||
                    EF.Functions.Like(s.RequestType.ToString(), $"%{searchString}%"));
            }

            var supportRequests = await supportRequestsQuery.OrderByDescending(r => r.CreatedAt).ToListAsync();
            ViewData["CurrentFilter"] = searchString;
            return View(supportRequests);
        }

        [Authorize(Policy = "CanAccessWorkQueue")]
        public async Task<IActionResult> WorkQueue(string searchString, SupportRequestStatus? selectedStatus)
        {
            var supportRequestsQuery = _context.SupportRequests
                                                .Include("CurrentApprover.User")
                                                .Include(r => r.ResponsibleUser)
                                                .AsQueryable();

            if (selectedStatus.HasValue)
            {
                supportRequestsQuery = supportRequestsQuery.Where(r => r.Status == selectedStatus.Value);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                 supportRequestsQuery = supportRequestsQuery.Where(s => 
                    EF.Functions.Like(s.RequesterName, $"%{searchString}%") ||
                    EF.Functions.Like(s.ProblemDescription, $"%{searchString}%") ||
                    EF.Functions.Like(s.RequestType.ToString(), $"%{searchString}%"));
            }

            var supportRequests = await supportRequestsQuery.OrderByDescending(r => r.CreatedAt).ToListAsync();
            var assignableUsers = await _context.Users.Where(u => u.Role == "ITSupport").ToListAsync();
            var filterableStatuses = Enum.GetValues(typeof(SupportRequestStatus)).Cast<SupportRequestStatus>().ToList();

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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Authorization Check
            if (!await IsAuthorized(id.Value))
            {
                return Forbid(); // Return 403 Forbidden if not authorized
            }

            var supportRequest = await _context.SupportRequests
                .Include(s => s.CurrentApprover).ThenInclude(a => a.User)
                .Include(s => s.ApprovalHistories)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supportRequest == null) return NotFound();
            
            return View(supportRequest); // Correctly returns the Details view
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
        public async Task<IActionResult> Create(SupportRequest supportRequest, IFormFile? attachmentFile)
        {
            // Conditional validation logic remains the same...
            bool isSoftwareRequest = supportRequest.RequestType == RequestType.Software;
            bool isSapProgram = isSoftwareRequest && supportRequest.Program == ProgramName.SAP;
            bool isSapRegistration = isSapProgram && supportRequest.SAPProblem == SAPProblemType.NewRegistration;
            bool isBomCreation = isSapProgram && supportRequest.SAPProblem == SAPProblemType.CreateBOM;

            if (!isSoftwareRequest) ModelState.Remove(nameof(SupportRequest.Program));
            if (!isSapProgram) ModelState.Remove(nameof(SupportRequest.SAPProblem));
            if (!isSapRegistration) RemoveSapRegistrationFields(ModelState);
            if (!isBomCreation) {
                ModelState.Remove(nameof(SupportRequest.BomComponents));
                foreach (var key in ModelState.Keys.Where(k => k.StartsWith("BomComponents[")).ToList()) ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    supportRequest.AttachmentPath = await SaveAttachment(attachmentFile);
                }

                var utcNow = DateTime.UtcNow;
                supportRequest.DocumentNo = await GenerateDocumentNo(utcNow);
                supportRequest.Status = SupportRequestStatus.Pending;
                supportRequest.CreatedAt = utcNow;
                supportRequest.UpdatedAt = utcNow;
                supportRequest.CreatedBy = User.Identity?.Name ?? "system";
                supportRequest.UpdatedBy = User.Identity?.Name ?? "system";

                _context.SupportRequests.Add(supportRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "บันทึกรายการแจ้งซ่อมสำเร็จแล้ว";
                return RedirectToAction(nameof(Index));
            }

            // If model state is invalid, return to the view with the model
            return View(supportRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Authorization Check
            if (!await IsAuthorized(id.Value))
            {
                return Forbid();
            }
            
            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null) return NotFound();

            // Additional check: Only allow editing for 'Pending' status
            if (supportRequest.Status != SupportRequestStatus.Pending) {
                TempData["ErrorMessage"] = "ไม่สามารถแก้ไขรายการที่อนุมัติไปแล้วได้";
                return RedirectToAction(nameof(Details), new { id = id.Value });
            }

            return View(supportRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupportRequest supportRequest, IFormFile? attachmentFile)
        {
            if (id != supportRequest.Id) return NotFound();

            // Authorization Check
            if (!await IsAuthorized(id)) return Forbid();

            var requestToUpdate = await _context.SupportRequests.FindAsync(id);
            if (requestToUpdate == null) return NotFound();
            
            // Ensure non-editable fields are not updated from the POSTed model
            // ... conditional validation logic ...

            if (ModelState.IsValid)
            {
                if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    requestToUpdate.AttachmentPath = await SaveAttachment(attachmentFile);
                }

                requestToUpdate.RequesterName = supportRequest.RequesterName;
                requestToUpdate.Department = supportRequest.Department;
                requestToUpdate.RequestType = supportRequest.RequestType;
                requestToUpdate.ProblemDescription = supportRequest.ProblemDescription;
                requestToUpdate.Urgency = supportRequest.Urgency;
                // Copy other editable fields...

                requestToUpdate.UpdatedAt = DateTime.UtcNow;
                requestToUpdate.UpdatedBy = User.Identity?.Name ?? "system";

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "แก้ไขรายการแจ้งซ่อมสำเร็จแล้ว";
                return RedirectToAction(nameof(Index));
            }
            return View(supportRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Authorization Check
            if (!await IsAuthorized(id.Value)) return Forbid();

            var supportRequest = await _context.SupportRequests.FirstOrDefaultAsync(m => m.Id == id);
            if (supportRequest == null) return NotFound();

            if (supportRequest.Status != SupportRequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "ไม่สามารถลบรายการที่อยู่ในขั้นตอนอนุมัติแล้วได้";
                return RedirectToAction(nameof(Index));
            }

            return View(supportRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Authorization Check
            if (!await IsAuthorized(id)) return Forbid();

            var supportRequest = await _context.SupportRequests.FindAsync(id);
            if (supportRequest == null) return NotFound();

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

        // --- Private Helper Methods ---
        private async Task<string> SaveAttachment(IFormFile attachmentFile)
        {
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(attachmentFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await attachmentFile.CopyToAsync(fileStream);
            }
            return "/uploads/" + uniqueFileName;
        }

        private async Task<string> GenerateDocumentNo(DateTime utcNow)
        {
            var thaiTime = utcNow.AddHours(7);
            var year = thaiTime.ToString("yy");
            var month = thaiTime.Month;
            string monthCode = month switch { 10 => "A", 11 => "B", 12 => "C", _ => month.ToString() };
            
            var startOfMonthThai = new DateTime(thaiTime.Year, thaiTime.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var startOfMonthUtc = startOfMonthThai.AddHours(-7);
            var endOfMonthUtc = startOfMonthUtc.AddMonths(1);

            var requestsInMonth = await _context.SupportRequests.CountAsync(r => r.CreatedAt >= startOfMonthUtc && r.CreatedAt < endOfMonthUtc);
            var runningNumber = requestsInMonth + 1;
            
            return $"SR-{year}{monthCode}-{runningNumber:D3}";
        }

        private void RemoveSapRegistrationFields(ModelStateDictionary modelState)
        {
            // This helper method remains the same
            var fields = new[] { nameof(SupportRequest.IsFG), nameof(SupportRequest.IsSM), /* ... all other SAP fields ... */ };
            foreach (var field in fields) modelState.Remove(field);
        }
    }
}
