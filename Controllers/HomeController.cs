using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Corrected to filter for "Published" status instead of "Active"
            var publishedNews = await _context.News
                                             .Where(n => n.Status == "Published")
                                             .OrderByDescending(n => n.PublishedDate)
                                             .ToListAsync();
            
            return View(publishedNews);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
