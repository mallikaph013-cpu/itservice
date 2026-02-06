using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using myapp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace myapp.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.FindFirst("IsDxStaff")?.Value != "True")
            {
                return Forbid();
            }

            var requests = await _context.SupportRequests
                .Include(s => s.ResponsibleUser) // Include the responsible user
                .Include(s => s.ApprovalHistories)
                .Include(s => s.CurrentApprover)
                    .ThenInclude(a => a!.User) 
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var reportViewModel = new ReportViewModel();

            foreach (var request in requests)
            {
                string resolutionTime = "N/A";
                if (request.Status == SupportRequestStatus.Done)
                {
                    var doneHistory = request.ApprovalHistories
                                             .OrderByDescending(h => h.ApprovalDate)
                                             .FirstOrDefault();
                    if (doneHistory != null)
                    {
                        var duration = doneHistory.ApprovalDate - request.CreatedAt;
                        resolutionTime = FormatDuration(duration);
                    }
                }
                else if (request.Status != SupportRequestStatus.Rejected)
                {
                    var duration = DateTime.UtcNow - request.CreatedAt.ToUniversalTime();
                    resolutionTime = $"Open for {FormatDuration(duration)}";
                }

                reportViewModel.ReportItems.Add(new ReportItemViewModel
                {
                    Request = request,
                    ResolutionTime = resolutionTime
                });
            }

            return View(reportViewModel);
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays} วัน {duration.Hours} ชั่วโมง";
            }
            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours} ชั่วโมง {duration.Minutes} นาที";
            }
            return $"{(int)duration.TotalMinutes} นาที {duration.Seconds} วินาที";
        }
    }
}
