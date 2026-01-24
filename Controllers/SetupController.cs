using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using System.Linq;
using System.IO;
using System;

namespace myapp.Controllers
{
    public class SetupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SetupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // A single, comprehensive action to diagnose and fix the admin password.
        public IActionResult DiagnoseAndFixAdmin()
        {
            string userId = "admin";
            string plainTextPassword = "12345@";
            string logContent = $"--- Admin Password Diagnosis & Fix Report ---\nTimestamp: {DateTime.UtcNow:O}\n";

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.EmployeeId == userId);

                if (user == null)
                {
                    logContent += "Result: FAILED\nError: User 'admin' not found in the database.";
                    System.IO.File.WriteAllText("setup_log.txt", logContent);
                    return Content("Action completed. User 'admin' was not found. Please check the setup_log.txt file. I will read it now.");
                }

                logContent += $"User 'admin' found. Current password value from DB: \"{user.Password}\"\n";
                bool isHashed = !string.IsNullOrEmpty(user.Password) && user.Password.Length >= 59 && user.Password.StartsWith("$2");

                if (isHashed)
                {
                    logContent += "Diagnosis: The current password is a valid BCrypt hash.\nAction: No action taken.";
                    System.IO.File.WriteAllText("setup_log.txt", logContent);
                    return Content("Action completed. Admin password is a valid hash. No changes made. I will now read the log.");
                }
                else
                {
                    logContent += "Diagnosis: The current password is NOT a valid hash (it is likely plain text).\n";
                    user.Password = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
                    _context.SaveChanges();

                    logContent += $"Action: Password has been successfully hashed and saved to the database.\nNew Hash: {user.Password}";
                    System.IO.File.WriteAllText("setup_log.txt", logContent);
                    return Content("Action completed. Admin password has been successfully hashed. I will now read the log to confirm.");
                }
            }
            catch (Exception ex)
            {
                logContent += $"Result: FAILED\nAn unexpected error occurred: {ex.Message}\nStack Trace: {ex.StackTrace}";
                System.IO.File.WriteAllText("setup_log.txt", logContent);
                return Content("An unexpected error occurred during the process. I will read the log file for details.");
            }
        }
    }
}
