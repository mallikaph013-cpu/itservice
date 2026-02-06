using Microsoft.EntityFrameworkCore;
using myapp.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace myapp.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Department> Departments { get; set; } = default!;
        public DbSet<ApprovalSequence> ApprovalSequences { get; set; } = default!;
        public DbSet<Approver> Approvers { get; set; } = default!;
        public DbSet<SupportRequest> SupportRequests { get; set; } = default!;
        public DbSet<ActivityLog> ActivityLogs { get; set; } = default!;
        public DbSet<News> News { get; set; } = default!;
        public DbSet<Menu> Menus { get; set; } = default!;
        public DbSet<UserMenuPermission> UserMenuPermissions { get; set; } = default!;
        public DbSet<WorkItem> WorkItems { get; set; } = default!;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = username;
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = username;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = username;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.ParentMenu)
                .WithMany(m => m.SubMenus)
                .HasForeignKey(m => m.ParentMenuId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            // Add unique constraint for UserId and MenuId in UserMenuPermission
            modelBuilder.Entity<UserMenuPermission>()
                .HasIndex(p => new { p.UserId, p.MenuId })
                .IsUnique();

            var utcNow = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new { Id = 1, Name = "IT", Status = "Active", CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 2, Name = "HR", Status = "Active", CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" }
            );

            // Seed Users with Hashed Password
            modelBuilder.Entity<User>().HasData(
                new
                {
                    Id = 1,
                    EmployeeId = "admin",
                    FirstName = "แอดมิน",
                    LastName = "ระบบ",
                    Password = "$2a$11$LhM27vKLaaDHqW9SJo3qoeswGqWwNQKADa6Z8CYfCg5NCk8UAFiHq", // "Admin@123"
                    Department = "IT",
                    Role = "Admin",
                    IsITStaff = false, // <-- I HAVE FIXED THIS
                    IsDxStaff = false,
                    CanApprove = true, // Added this line
                    CreatedAt = utcNow,
                    UpdatedAt = utcNow,
                    CreatedBy = "system",
                    UpdatedBy = "system"
                }
            );

            // Seed Menus
            modelBuilder.Entity<Menu>().HasData(
                new { Id = 1, Name = "หน้าหลัก", ControllerName = "Home", ActionName = "Index", IsDropdown = false, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 2, Name = "ข่าวสาร", ControllerName = "News", ActionName = "Index", IsDropdown = false, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 3, Name = "แจ้งซ่อม", ControllerName = "ITSupport", ActionName = "Index", IsDropdown = false, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 4, Name = "จัดการข้อมูล", IsDropdown = true, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 5, Name = "ข้อมูลฝ่าย", ControllerName = "Departments", ActionName = "Index", ParentMenuId = 4, IsDropdown = false, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 6, Name = "ข้อมูลผู้ใช้งาน", ControllerName = "Users", ActionName = "Index", ParentMenuId = 4, IsDropdown = false, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 7, Name = "ลำดับการอนุมัติ", ControllerName = "ApprovalSequences", ActionName = "Index", ParentMenuId = 4, IsDropdown = false, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 8, Name = "ประวัติการใช้งาน", ControllerName = "ActivityLog", ActionName = "Index", IsDropdown = false, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" },
                new { Id = 9, Name = "จัดการสิทธิ์เมนู", ControllerName = "UserMenu", ActionName = "Index", ParentMenuId = 4, IsDropdown = false, CreatedAt = utcNow, UpdatedAt = utcNow, CreatedBy = "system", UpdatedBy = "system" }
            );
        }
    }
}
