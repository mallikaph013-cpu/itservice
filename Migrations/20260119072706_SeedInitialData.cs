using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Name", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "IT", "Active", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "HR", "Active", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system" }
                });

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

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Department", "EmployeeId", "FirstName", "LastName", "Password", "Role" },
                values: new object[] { 1, "IT", "admin", "แอดมิน", "ระบบ", "$2a$11$k.MA.p9hM5w2vV.J/B4xFeV7U5sZp6PzEd3AAl2vE6n3g2gT2mY0O", "Admin" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
