using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using myapp.ViewModels; 
using System.Collections.Generic; 
using System.Threading.Tasks;

namespace myapp.Controllers
{
    public class WorkReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Step 1: Fetch the necessary data from the database to the client.
            // We include ResponsibleUser to ensure the data is loaded.
            var completedRequests = await _context.SupportRequests
                .Include(s => s.ResponsibleUser) // Eagerly load the related User
                .Where(s => s.Status == Models.SupportRequestStatus.Done && s.ResponsibleUser != null)
                .ToListAsync();

            // Step 2: Now that the data is in memory, perform the GroupBy operation.
            // This will use LINQ to Objects, which can handle the calculated FullName property.
            var report = completedRequests
                .GroupBy(s => s.ResponsibleUser!.FullName)
                .Select(g => new WorkReportViewModel
                {
                    ResponsibleUserName = g.Key,
                    CompletedCount = g.Count()
                })
                .OrderByDescending(vm => vm.CompletedCount)
                .ToList();

            return View(report);
        }
    }
}
