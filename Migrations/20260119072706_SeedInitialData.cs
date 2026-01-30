using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace myapp.Migrations
{
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Departments
            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Name", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "IT", "Active", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "HR", "Active", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system" }
                });

            // Seed Users with plain text passwords for initial setup
            // The login logic will hash them on first login.
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Department", "EmployeeId", "FirstName", "LastName", "Password", "Role" },
                values: new object[,]
                {
                    { 1, "IT", "admin", "แอดมิน", "ระบบ", "1234", "Admin" },
                    { 2, "HR", "user", "ผู้ใช้", "ทั่วไป", "1234", "User" },
                    { 3, "IT", "it_user", "ผู้ใช้", "ไอที", "1234", "IT" }
                });

            // Seed Menus
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "ActionName", "ControllerName", "IsDropdown", "Name", "ParentMenuId" },
                values: new object[,]
                {
                    { 1, "Index", "Home", false, "หน้าหลัก", null },
                    { 2, "Index", "News", false, "ข่าวสาร", null },
                    { 3, "Index", "ITSupport", false, "แจ้งซ่อม", null },
                    { 4, null, null, true, "จัดการข้อมูล", null },
                    { 8, "Index", "ActivityLog", false, "ประวัติการใช้งาน", null }
                });

            // Seed Sub-Menus
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "ActionName", "ControllerName", "IsDropdown", "Name", "ParentMenuId" },
                values: new object[,]
                {
                    { 5, "Index", "Departments", false, "ข้อมูลฝ่าย", 4 },
                    { 6, "Index", "Users", false, "ข้อมูลผู้ใช้งาน", 4 },
                    { 7, "Index", "ApprovalSequences", false, "ลำดับการอนุมัติ", 4 },
                    { 9, "Index", "RoleMenu", false, "จัดการสิทธิ์เมนู", 4 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all seeded data in reverse order of creation
            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValues: new object[] { 5, 6, 7, 9 });

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 8 });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3 });

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 4); // Parent menu deleted last
        }
    }
}
