using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var activeNews = await _context.News
                                             .Where(n => n.Status == "Active")
                                             .OrderByDescending(n => n.PublishedDate)
                                             .ToListAsync();
            
            return View(activeNews);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
