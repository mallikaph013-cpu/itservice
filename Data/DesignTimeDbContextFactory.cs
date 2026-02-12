using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace myapp.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var provider = configuration.GetValue("DatabaseProvider", "SQLite");

            if (provider == "SQLServer")
            {
                var connectionString = configuration.GetConnectionString("SQLServer");
                builder.UseSqlServer(connectionString, 
                    x => x.MigrationsAssembly("myapp"));
            }
            else
            {
                var connectionString = configuration.GetConnectionString("SQLite");
                builder.UseSqlite(connectionString, 
                    x => x.MigrationsAssembly("myapp"));
            }

            // Create a mock HttpContextAccessor for design time
            var httpContextAccessor = new HttpContextAccessor();

            return new ApplicationDbContext(builder.Options, httpContextAccessor);
        }
    }
}
