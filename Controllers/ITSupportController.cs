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
                                                .Include("CurrentApprover.User")
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
                                                .Include("CurrentApprover.User")
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
        public async Task<IActionResult> Create(SupportRequest supportRequest, IFormFile? attachmentFile)
        {
            // --- Start of Conditional Validation ---
            bool isSoftwareRequest = supportRequest.RequestType == RequestType.Software;
            bool isSapProgram = isSoftwareRequest && supportRequest.Program == ProgramName.SAP;
            bool isSapRegistration = isSapProgram && supportRequest.SAPProblem == SAPProblemType.NewRegistration;
            bool isBomCreation = isSapProgram && supportRequest.SAPProblem == SAPProblemType.CreateBOM;

            if (!isSoftwareRequest)
            {
                ModelState.Remove(nameof(SupportRequest.Program));
            }

            if (!isSapProgram)
            {
                ModelState.Remove(nameof(SupportRequest.SAPProblem));
            }

            if (!isSapRegistration)
            {
                RemoveSapRegistrationFields(ModelState);
            }

            if (!isBomCreation)
            {
                ModelState.Remove(nameof(SupportRequest.BomComponents));
                 // Also remove individual items in case they are added to ModelState
                foreach (var key in ModelState.Keys.Where(k => k.StartsWith("BomComponents[")).ToList())
                {
                    ModelState.Remove(key);
                }
            }
            // --- End of Conditional Validation ---

            if (ModelState.IsValid)
            {
                try 
                {
                    if (attachmentFile != null && attachmentFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(attachmentFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await attachmentFile.CopyToAsync(fileStream);
                        }
                        supportRequest.AttachmentPath = "/uploads/" + uniqueFileName; 
                    }

                    // --- DocumentNo Generation (Robust, No TimeZoneInfo) ---
                    var utcNow = DateTime.UtcNow;
                    var thaiTime = utcNow.AddHours(7); // Simple and reliable conversion to Thai time

                    var year = thaiTime.ToString("yy");
                    var month = thaiTime.Month;
                    string monthCode;
                    switch (month)
                    {
                        case 10: monthCode = "A"; break;
                        case 11: monthCode = "B"; break;
                        case 12: monthCode = "C"; break;
                        default: monthCode = month.ToString(); break;
                    }

                    // Determine the start and end of the current month in UTC
                    var startOfMonthThai = new DateTime(thaiTime.Year, thaiTime.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
                    var startOfMonthUtc = startOfMonthThai.AddHours(-7);
                    var endOfMonthUtc = startOfMonthUtc.AddMonths(1);

                    var requestsInMonth = await _context.SupportRequests
                        .CountAsync(r => r.CreatedAt >= startOfMonthUtc && r.CreatedAt < endOfMonthUtc);

                    var runningNumber = requestsInMonth + 1;
                    
                    supportRequest.DocumentNo = $"SR-{year}{monthCode}-{runningNumber:D3}";
                    // --- End of DocumentNo Generation ---


                    supportRequest.Status = SupportRequestStatus.Pending;
                    supportRequest.CurrentApproverId = null;

                    supportRequest.CreatedAt = utcNow;
                    supportRequest.UpdatedAt = utcNow;
                    supportRequest.CreatedBy = User.Identity?.Name ?? "system";
                    supportRequest.UpdatedBy = User.Identity?.Name ?? "system";

                    _context.SupportRequests.Add(supportRequest);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "บันทึกรายการแจ้งซ่อมสำเร็จแล้ว เอกสารจะถูกส่งไปตามลำดับการอนุมัติ";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                    TempData["ValidationErrors"] = $"Database Save Error: {innerExceptionMessage}";
                    return View(supportRequest);
                }
            }
            else
            {
                var errorsWithKeys = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var errorMessage = "Validation Error: ";
                foreach (var error in errorsWithKeys)
                {
                    errorMessage += $"Field: '{error.Key}', Problem: '{string.Join(", ", error.Value)}'; ";
                }
                
                TempData["ValidationErrors"] = errorMessage;
                 return View(supportRequest);
            }
        }

        private void RemoveSapRegistrationFields(ModelStateDictionary modelState)
        {
            modelState.Remove(nameof(SupportRequest.IsFG));
            modelState.Remove(nameof(SupportRequest.IsSM));
            modelState.Remove(nameof(SupportRequest.IsRM));
            modelState.Remove(nameof(SupportRequest.IsTooling));
            modelState.Remove(nameof(SupportRequest.ICSCode));
            modelState.Remove(nameof(SupportRequest.EnglishMatDescription));
            modelState.Remove(nameof(SupportRequest.MaterialGroup));
            modelState.Remove(nameof(SupportRequest.Division));
            modelState.Remove(nameof(SupportRequest.ProfitCenter));
            modelState.Remove(nameof(SupportRequest.DistributionChannel));
            modelState.Remove(nameof(SupportRequest.BOICode));
            modelState.Remove(nameof(SupportRequest.MRPController));
            modelState.Remove(nameof(SupportRequest.StorageLoc));
            modelState.Remove(nameof(SupportRequest.StorageLocBP));
            modelState.Remove(nameof(SupportRequest.StorageLocB1));
            modelState.Remove(nameof(SupportRequest.ProductionSupervisor));
            modelState.Remove(nameof(SupportRequest.CostingLotSize));
            modelState.Remove(nameof(SupportRequest.ValClass));
            modelState.Remove(nameof(SupportRequest.Plant));
            modelState.Remove(nameof(SupportRequest.BaseUnit));
            modelState.Remove(nameof(SupportRequest.MakerMfrPartNumber));
            modelState.Remove(nameof(SupportRequest.Price));
            modelState.Remove(nameof(SupportRequest.PerPrice));
            modelState.Remove(nameof(SupportRequest.ModelName));
            modelState.Remove(nameof(SupportRequest.BOIDescription));
            modelState.Remove(nameof(SupportRequest.PurchasingGroup));
            modelState.Remove(nameof(SupportRequest.CommCodeTariffCode));
            modelState.Remove(nameof(SupportRequest.TariffCodePercentage));
            modelState.Remove(nameof(SupportRequest.PriceControl));
            modelState.Remove(nameof(SupportRequest.SupplierCode));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supportRequest = await _context.SupportRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supportRequest == null)
            {
                return NotFound();
            }

            ViewData["IsReadOnly"] = true;
            return View("Edit", supportRequest);
        }

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

            return View(supportRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupportRequest supportRequest, IFormFile? attachmentFile)
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

            // Remove validation for conditionally hidden fields
            if (supportRequest.RequestType != RequestType.Software)
            {
                ModelState.Remove(nameof(SupportRequest.Program));
                ModelState.Remove(nameof(SupportRequest.SAPProblem));
            }
            else if (supportRequest.Program != ProgramName.SAP)
            {
                ModelState.Remove(nameof(SupportRequest.SAPProblem));
            }

            if (ModelState.IsValid)
            {
                 if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(attachmentFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachmentFile.CopyToAsync(fileStream);
                    }
                    requestToUpdate.AttachmentPath = "/uploads/" + uniqueFileName; 
                }

                requestToUpdate.EmployeeId = supportRequest.EmployeeId;
                requestToUpdate.RequesterName = supportRequest.RequesterName;
                requestToUpdate.Department = supportRequest.Department;
                requestToUpdate.RequestType = supportRequest.RequestType;
                requestToUpdate.Program = supportRequest.Program;
                requestToUpdate.SAPProblem = supportRequest.SAPProblem;
                requestToUpdate.ProblemDescription = supportRequest.ProblemDescription;
                requestToUpdate.Urgency = supportRequest.Urgency;

                requestToUpdate.UpdatedAt = DateTime.UtcNow;
                requestToUpdate.UpdatedBy = User.Identity?.Name ?? "system";

                try
                {
                    _context.Update(requestToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SupportRequests.Any(e => e.Id == supportRequest.Id))
                    {
                        return NotFound();
                    } else { throw; }
                }
                TempData["SuccessMessage"] = "แก้ไขรายการแจ้งซ่อมสำเร็จแล้ว";
                return RedirectToAction(nameof(Index));
            }
            return View(supportRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) { return NotFound(); }

            var supportRequest = await _context.SupportRequests
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supportRequest == null) { return NotFound(); }

            if (supportRequest.Status != SupportRequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "ไม่สามารถลบรายการที่อนุมัติแล้วได้";
                return RedirectToAction(nameof(Index));
            }

            return View(supportRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supportRequest = await _context.SupportRequests.FindAsync(id);
             if (supportRequest == null) { return NotFound(); }

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