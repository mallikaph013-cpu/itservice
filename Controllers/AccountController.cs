using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using myapp.Models;
using myapp.Services;
using myapp.Data;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;

namespace myapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLogService;

        public AccountController(ApplicationDbContext context, ActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.EmployeeId == model.UserName);

                if (user != null)
                {
                    bool isPasswordValid = false;
                    
                    // A BCrypt hash will start with a prefix like $2a$, $2b$, or $2y$.
                    // We can check this to reliably determine if the password is hashed.
                    bool isHashed = user.Password.StartsWith("$2");

                    if (isHashed)
                    {
                        // The password in the DB is already hashed. Verify it directly.
                        try
                        {
                            isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
                        }
                        catch
                        {
                            // If any error occurs during verification of a supposed hash, treat as invalid.
                            isPasswordValid = false;
                        }
                    }
                    else
                    {
                        // This is a plain-text password. Compare it directly.
                        if (user.Password == model.Password)
                        {
                            // The password is correct. Mark it as valid for this request.
                            isPasswordValid = true;
                            
                            // Now, hash the password and update it in the database for future logins.
                            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                            _context.Users.Update(user);
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (isPasswordValid)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.EmployeeId),
                            new Claim(ClaimTypes.NameIdentifier, user.EmployeeId),
                            new Claim(ClaimTypes.Role, user.Role),
                        };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        await _activityLogService.LogActivityAsync("User Login", $"User '{user.EmployeeId}' logged in successfully.");

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                // If user is null or password is not valid
                await _activityLogService.LogActivityAsync("Login Failure", $"Failed login attempt for EmployeeId '{model.UserName}'.");
                ModelState.AddModelError(string.Empty, "รหัสพนักงานหรือรหัสผ่านไม่ถูกต้อง");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await _activityLogService.LogActivityAsync("User Logout", $"User '{userId}' logged out.");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
