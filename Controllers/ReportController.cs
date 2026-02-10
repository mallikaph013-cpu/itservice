
using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using myapp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.IO;
using QuestPDF.Fluent;
using myapp.Documents;

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
            // Allow Admins OR users who are DX Staff to access this report.
            if (!User.IsInRole("Admin") && User.FindFirst("IsDxStaff")?.Value != "True")
            {
                return Forbid();
            }

            var requests = await _context.SupportRequests
                .Include(s => s.ResponsibleUser) 
                .Include(s => s.ApprovalHistories)
                .Include(s => s.CurrentApprover)
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

        public async Task<IActionResult> GeneratePdf(int? id)
        {
            if (!User.IsInRole("Admin") && User.FindFirst("IsDxStaff")?.Value != "True")
            {
                return Forbid();
            }

            if (id.HasValue)
            {
                // Generate a single request form
                var request = await _context.SupportRequests
                    .Include(s => s.ResponsibleUser)
                    .Include(s => s.ApprovalHistories)
                    .FirstOrDefaultAsync(s => s.Id == id.Value);

                if (request == null)
                {
                    return NotFound();
                }

                var document = new SupportRequestDocument(request);
                var pdfData = document.GeneratePdf();
                return File(pdfData, "application/pdf", $"SupportRequest_{id.Value}.pdf");
            }
            else
            {
                // Generate the summary report
                var requests = await _context.SupportRequests
                    .Include(s => s.ResponsibleUser)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                var reportViewModel = new ReportViewModel();
                foreach (var request in requests)
                {
                    reportViewModel.ReportItems.Add(new ReportItemViewModel
                    {
                        Request = request,
                        ResolutionTime = "N/A" // This will be calculated in the view
                    });
                }

                var document = new ReportDocument(reportViewModel);
                var pdfData = document.GeneratePdf();
                return File(pdfData, "application/pdf", "SupportRequestsReport.pdf");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            if (!User.IsInRole("Admin") && User.FindFirst("IsDxStaff")?.Value != "True")
            {
                return Forbid();
            }

            var requests = await _context.SupportRequests
                .Include(s => s.ResponsibleUser)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SupportRequests");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "ผู้แจ้ง";
                worksheet.Cell(currentRow, 2).Value = "แผนก";
                worksheet.Cell(currentRow, 3).Value = "ประเภท";
                worksheet.Cell(currentRow, 4).Value = "ผู้รับผิดชอบ";
                worksheet.Cell(currentRow, 5).Value = "ความเร่งด่วน";
                worksheet.Cell(currentRow, 6).Value = "สถานะ";
                worksheet.Cell(currentRow, 7).Value = "วันที่แจ้ง";
                worksheet.Cell(currentRow, 8).Value = "วันที่เสร็จสิ้น";
                worksheet.Cell(currentRow, 9).Value = "ระยะเวลา";

                foreach (var item in requests)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.RequesterName;
                    worksheet.Cell(currentRow, 2).Value = item.Department;
                    worksheet.Cell(currentRow, 3).Value = item.RequestType.ToString();
                    worksheet.Cell(currentRow, 4).Value = item.ResponsibleUser?.FullName ?? "-";
                    worksheet.Cell(currentRow, 5).Value = item.Urgency.ToString();
                    worksheet.Cell(currentRow, 6).Value = item.Status.ToString();
                    worksheet.Cell(currentRow, 7).Value = item.CreatedAt;
                    worksheet.Cell(currentRow, 8).Value = item.Status == SupportRequestStatus.Done ? item.UpdatedAt.ToString("dd/MM/yy HH:mm") : "-";
                    
                    string resolutionTimeDisplay = "-";
                    if (item.Status == SupportRequestStatus.Done)
                    {
                        TimeSpan duration = item.UpdatedAt - item.CreatedAt;
                        var parts = new List<string>();
                        if (duration.Days > 0) parts.Add($"{duration.Days} วัน");
                        if (duration.Hours > 0) parts.Add($"{duration.Hours} ชั่วโมง");
                        if (duration.Minutes > 0) parts.Add($"{duration.Minutes} นาที");
                        resolutionTimeDisplay = parts.Any() ? string.Join(" ", parts) : "< 1 นาที";
                    }
                    worksheet.Cell(currentRow, 9).Value = resolutionTimeDisplay;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "SupportRequestsReport.xlsx");
                }
            }
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
