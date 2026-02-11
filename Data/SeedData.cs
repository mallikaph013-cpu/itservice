using Microsoft.EntityFrameworkCore;
using myapp.Models;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Collections.Generic;

namespace myapp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            // --- Seed Users ---
            var initialUsers = new[]
            {
                new { EmployeeId = "admin", Password = "admin123", FirstName = "Admin", LastName = "User", Role = "Admin", Department = "IT", Section = "Section A", IsITStaff = false, IsDxStaff = true, CanApprove = true },
                new { EmployeeId = "itsupport1", Password = "support123", FirstName = "IT", LastName = "Support 1", Role = "ITSupport", Department = "IT", Section = "Section A", IsITStaff = true, IsDxStaff = true, CanApprove = false },
                new { EmployeeId = "006038", Password = "1234", FirstName = "Test", LastName = "User", Role = "User", Department = "IT", Section = "Section B", IsITStaff = false, IsDxStaff = false, CanApprove = true },
            };

            foreach (var userData in initialUsers)
            {
                if (!await context.Users.AnyAsync(u => u.EmployeeId == userData.EmployeeId))
                {
                    context.Users.Add(new User
                    {
                        EmployeeId = userData.EmployeeId,
                        Password = BCrypt.Net.BCrypt.HashPassword(userData.Password),
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        Role = userData.Role,
                        Department = userData.Department,
                        Section = userData.Section,
                        IsITStaff = userData.IsITStaff,
                        IsDxStaff = userData.IsDxStaff,
                        CanApprove = userData.CanApprove
                    });
                }
            }
            await context.SaveChangesAsync(); // Save users to get their Ids

            // --- Seed Departments ---
            var initialDepartments = new[] { "IT", "HR", "Finance", "Production", "Engineering", "Sales", "Marketing", "Accounting", "Quality Assurance" };
            foreach (var deptName in initialDepartments)
            {
                if (!await context.Departments.AnyAsync(d => d.Name == deptName))
                {
                    context.Departments.Add(new Department { Name = deptName, Status = "Active" });
                }
            }
            await context.SaveChangesAsync();

            // --- Seed Sections ---
            var initialSections = new[] { "Section A", "Section B", "Section C" };
            foreach (var sectionName in initialSections)
            {
                if (!await context.Sections.AnyAsync(s => s.Name == sectionName))
                {
                    context.Sections.Add(new Section { Name = sectionName, Status = "Active" });
                }
            }
            await context.SaveChangesAsync();

            // --- Seed IT Approval Sequence ---
            var itDepartment = "IT";
            var itSequence = await context.ApprovalSequences.FirstOrDefaultAsync(s => s.Department == itDepartment);

            if (itSequence == null)
            {
                itSequence = new ApprovalSequence { Department = itDepartment, Approvers = new List<Approver>() };
                context.ApprovalSequences.Add(itSequence);
                await context.SaveChangesAsync(); // Save sequence to get its Id
            }

            // --- Seed Approver for IT Sequence ---
            var approverUser = await context.Users.FirstOrDefaultAsync(u => u.EmployeeId == "006038");
            if (approverUser != null)
            {
                // Check if this user is already an approver for this sequence
                bool isAlreadyApprover = await context.Approvers.AnyAsync(a => a.ApprovalSequenceId == itSequence.Id && a.UserId == approverUser.Id);
                
                if (!isAlreadyApprover)
                {
                    context.Approvers.Add(new Approver { ApprovalSequenceId = itSequence.Id, UserId = approverUser.Id, Order = 1 });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
