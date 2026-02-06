using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using myapp.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    public class SummaryReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SummaryReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var summaryByRequestType = await _context.SupportRequests
                .GroupBy(s => s.RequestType)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var summaryByStatus = await _context.SupportRequests
                .GroupBy(s => s.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var viewModel = new SummaryReportViewModel
            {
                SummaryByRequestType = summaryByRequestType,
                SummaryByStatus = summaryByStatus
            };

            return View(viewModel);
        }
    }
}
