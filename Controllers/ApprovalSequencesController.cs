using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace myapp.Controllers
{
    public class ApprovalSequencesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApprovalSequencesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ApprovalSequences
        public async Task<IActionResult> Index()
        {
            var approvalSequences = await _context.ApprovalSequences.Include(a => a.Approvers).ThenInclude(approver => approver.User).ToListAsync();
            return View(approvalSequences);
        }

        // GET: ApprovalSequences/Create
        public async Task<IActionResult> Create()
        {
            await PopulateViewData(null);
            return View();
        }

        // POST: ApprovalSequences/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Department,Section,Status")] ApprovalSequence approvalSequence, int[] approverIds, int[] approverOrders)
        {
            if (ModelState.IsValid)
            {
                approvalSequence.CreatedAt = DateTime.UtcNow;
                approvalSequence.UpdatedAt = DateTime.UtcNow;
                approvalSequence.CreatedBy = "system"; // Should be replaced with actual user
                approvalSequence.UpdatedBy = "system"; // Should be replaced with actual user

                if (approverIds != null && approverOrders != null)
                {
                    int count = Math.Min(approverIds.Length, approverOrders.Length);
                    for (int i = 0; i < count; i++)
                    {
                        if (approverIds[i] > 0 && approverOrders[i] > 0)
                        {
                            approvalSequence.Approvers.Add(new Approver { UserId = approverIds[i], Order = approverOrders[i] });
                        }
                    }
                }

                _context.Add(approvalSequence);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something failed, redisplay form
            // Preserve the user's selected approvers so they don't lose their work.
            if (approverIds != null && approverOrders != null)
            {
                int count = Math.Min(approverIds.Length, approverOrders.Length);
                ViewData["SelectedApprovers"] = Enumerable.Range(0, count)
                    .Select(i => new { UserId = approverIds[i], Order = approverOrders[i] })
                    .Where(a => a.UserId > 0)
                    .ToList();
            }

            await PopulateViewData(approvalSequence);
            return View(approvalSequence);
        }

        // GET: ApprovalSequences/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approvalSequence = await _context.ApprovalSequences.Include(s => s.Approvers).ThenInclude(a => a.User).FirstOrDefaultAsync(s => s.Id == id);
            if (approvalSequence == null)
            {
                return NotFound();
            }
            await PopulateViewData(approvalSequence);
            return View(approvalSequence);
        }

        // POST: ApprovalSequences/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Department,Section,Status")] ApprovalSequence approvalSequence, int[] approverIds, int[] approverOrders)
        {
            if (id != approvalSequence.Id)
            {
                return NotFound();
            }

            // Manually check for required Department as model state might be tricky with dynamic fields
            if (string.IsNullOrEmpty(approvalSequence.Department))
            {
                 ModelState.AddModelError("Department", "The Department field is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var sequenceToUpdate = await _context.ApprovalSequences.Include(s => s.Approvers).FirstOrDefaultAsync(s => s.Id == id);
                    if (sequenceToUpdate == null) return NotFound();

                    sequenceToUpdate.Department = approvalSequence.Department;
                    sequenceToUpdate.Section = approvalSequence.Section;
                    sequenceToUpdate.Status = approvalSequence.Status;
                    sequenceToUpdate.UpdatedAt = DateTime.UtcNow;
                    sequenceToUpdate.UpdatedBy = "system"; // Should be replaced with actual user

                    // Update approvers
                    sequenceToUpdate.Approvers.Clear();
                     if (approverIds != null && approverOrders != null)
                    {
                        int count = Math.Min(approverIds.Length, approverOrders.Length);
                        for (int i = 0; i < count; i++)
                        {
                            if (approverIds[i] > 0 && approverOrders[i] > 0)
                            {
                                sequenceToUpdate.Approvers.Add(new Approver { UserId = approverIds[i], Order = approverOrders[i] });
                            }
                        }
                    }

                    _context.Update(sequenceToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.ApprovalSequences.AnyAsync(e => e.Id == approvalSequence.Id))
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
            
            // If we got this far, something failed, redisplay form
            // Preserve the user's selected approvers so they don't lose their work.
             if (approverIds != null && approverOrders != null)
            {
                int count = Math.Min(approverIds.Length, approverOrders.Length);
                ViewData["SelectedApprovers"] = Enumerable.Range(0, count)
                    .Select(i => new { UserId = approverIds[i], Order = approverOrders[i] })
                    .Where(a => a.UserId > 0)
                    .ToList();
            }

            await PopulateViewData(approvalSequence);
            return View(approvalSequence);
        }

        // POST: ApprovalSequences/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var approvalSequence = await _context.ApprovalSequences.FindAsync(id);
            if (approvalSequence != null)
            {
                _context.ApprovalSequences.Remove(approvalSequence);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetSections(string department)
        {
            var sections = await _context.Sections
                                         .Where(s => s.Department.Name == department && s.Status == "Active")
                                         .OrderBy(s => s.Name)
                                         .Select(s => new { s.Id, s.Name })
                                         .ToListAsync();
            return Json(sections);
        }

        // Helper method to populate ViewData
        private async Task PopulateViewData(ApprovalSequence? sequence)
        {
            ViewData["Departments"] = new SelectList(await _context.Departments.Where(d => d.Status == "Active").OrderBy(d=> d.Name).ToListAsync(), "Name", "Name", sequence?.Department);
            if (sequence?.Department != null)
            {
                ViewData["Sections"] = new SelectList(await _context.Sections.Where(s => s.Department.Name == sequence.Department && s.Status == "Active").OrderBy(s => s.Name).ToListAsync(), "Name", "Name", sequence?.Section);
            }
            else
            {
                ViewData["Sections"] = new SelectList(new List<SelectListItem>(), "Name", "Name");
            }
            
            var users = await _context.Users.Where(u => u.Role != "Admin").OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToListAsync();
            ViewData["Users"] = users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName }).ToList();
        }
    }
}
