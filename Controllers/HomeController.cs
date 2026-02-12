using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace myapp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("ITSupport"))
            {
                return RedirectToAction("Index", "ITSupport");
            }
            else if (User.IsInRole("User"))
            {
                return RedirectToAction("Create", "ITSupport");
            }
            
            // If user has no specific role, redirect to a generic page or show an error
            return RedirectToAction("AccessDenied", "Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
