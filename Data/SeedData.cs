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
            // Look for any menus.
            if (context.Menus.Any(m => m.Name == "รับงาน"))
            {
                return;   // DB has been seeded
            }

            var newMenu = new Menu
            {
                Name = "รับงาน",
                ControllerName = "WorkItem",
                ActionName = "Index",
                IsDropdown = false
            };

            context.Menus.Add(newMenu);
            await context.SaveChangesAsync();
        }
    }
}
