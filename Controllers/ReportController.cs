using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using myapp.Models;
using QuestPDF.Fluent;
using myapp.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using myapp.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace myapp.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Report
        [Authorize]
        public async Task<IActionResult> Index(SupportRequestStatus? status)
        {
            IQueryable<SupportRequest> query = _context.SupportRequests.Include(s => s.ResponsibleUser);

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            var requests = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
            var allRequests = await _context.SupportRequests.ToListAsync();

            var viewModel = new ReportIndexViewModel
            {
                Requests = requests,
                SelectedStatus = status,
                StatusOptions = new SelectList(Enum.GetValues(typeof(SupportRequestStatus)).Cast<SupportRequestStatus>().Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }), "Value", "Text", status),
                TotalRequests = allRequests.Count,
                PendingRequests = allRequests.Count(s => s.Status == SupportRequestStatus.Pending),
                InProgressRequests = allRequests.Count(s => s.Status == SupportRequestStatus.InProgress),
                DoneRequests = allRequests.Count(s => s.Status == SupportRequestStatus.Done)
            };

            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> GeneratePdf(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supportRequest = await _context.SupportRequests
                .Include(s => s.ResponsibleUser)
                .Include(s => s.ApprovalHistories)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supportRequest == null)
            {
                return NotFound();
            }

            var approverNames = supportRequest.ApprovalHistories
                                              .Select(h => h.ApproverName)
                                              .Distinct()
                                              .ToList();

            var approvers = await _context.Users
                                          .Where(u => approverNames.Contains(u.FullName))
                                          .ToListAsync();

            var document = new SupportRequestDocument(supportRequest, approvers);
            var pdfData = document.GeneratePdf();

            return File(pdfData, "application/pdf", $"SupportRequest-{supportRequest.Id}.pdf");
        }
    }
}
