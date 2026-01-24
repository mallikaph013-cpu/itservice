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

        public async Task<IActionResult> Index()
        {
            var supportRequests = await _context.SupportRequests
                                                .Include(r => r.CurrentApprover) // Eager load the approver
                                                .OrderByDescending(r => r.CreatedAt)
                                                .ToListAsync();
            return View(supportRequests);
        }

        [Authorize(Roles = "ITSupport,Admin")]
        public async Task<IActionResult> WorkQueue()
        {   
            var supportRequests = await _context.SupportRequests
                                                .Include(r => r.CurrentApprover) // Eager load the approver
                                                .OrderByDescending(r => r.CreatedAt)
                                                .ToListAsync();
            return View(supportRequests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ITSupport,Admin")]
        public async Task<IActionResult> UpdateStatus(int id, SupportRequestStatus status)
        {
            var request = await _context.SupportRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            request.UpdatedAt = DateTime.UtcNow;
            request.UpdatedBy = User.Identity?.Name; 

            _context.Update(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "อัปเดตสถานะเรียบร้อยแล้ว";

            return RedirectToAction(nameof(WorkQueue));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupportRequest supportRequest)
        {
            if (ModelState.IsValid)
            {
                supportRequest.CreatedAt = DateTime.UtcNow;
                supportRequest.UpdatedAt = DateTime.UtcNow;
                supportRequest.CreatedBy = User.Identity?.Name; 
                supportRequest.UpdatedBy = User.Identity?.Name;

                if (!string.IsNullOrEmpty(supportRequest.Department))
                {
                    var approvalSequence = await _context.ApprovalSequences
                                               .Include(a => a.Approvers) 
                                               .FirstOrDefaultAsync(a => string.Equals(a.Department, supportRequest.Department, StringComparison.OrdinalIgnoreCase));

                    if (approvalSequence != null && approvalSequence.Approvers.Any())
                    {
                        var firstApprover = approvalSequence.Approvers.OrderBy(ap => ap.Order).FirstOrDefault();
                        if (firstApprover != null)
                        {
                            supportRequest.Status = SupportRequestStatus.Pending;
                            supportRequest.CurrentApproverId = firstApprover.Id;
                        }
                        else
                        {
                             supportRequest.Status = SupportRequestStatus.Approved; 
                             supportRequest.CurrentApproverId = null;
                        }
                    }
                    else
                    {
                        supportRequest.Status = SupportRequestStatus.Approved; 
                        supportRequest.CurrentApproverId = null;
                    }
                }
                else
                {
                     supportRequest.Status = SupportRequestStatus.Approved; 
                     supportRequest.CurrentApproverId = null;
                }

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
                requestToUpdate.UpdatedBy = User.Identity?.Name;

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
