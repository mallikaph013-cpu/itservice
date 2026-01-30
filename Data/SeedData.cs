using Microsoft.EntityFrameworkCore;
using myapp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace myapp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new User[]
            {
                new User { EmployeeId = "admin", Password = BCrypt.Net.BCrypt.HashPassword("1234"), FirstName = "Admin", LastName = "User", Role = "Admin", Department = "IT" },
                new User { EmployeeId = "006038", Password = BCrypt.Net.BCrypt.HashPassword("1234"), FirstName = "Test", LastName = "User", Role = "User", Department = "IT" },
            };

            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            await context.SaveChangesAsync();
        }
    }
}
