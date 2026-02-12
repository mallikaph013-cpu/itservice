using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using System.Linq;

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

        public IActionResult Index()
        {
            var news = _context.News.OrderByDescending(n => n.PublishedDate).ToList();
            return View(news);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
