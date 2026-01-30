using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "WorkItems",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "WorkItems",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WorkItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "WorkItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserMenuPermissions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserMenuPermissions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserMenuPermissions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "UserMenuPermissions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "News",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "News",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "News",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "News",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Approvers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Approvers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Approvers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Approvers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ApprovalHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ApprovalHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ApprovalHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ApprovalHistory",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserMenuPermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserMenuPermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserMenuPermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserMenuPermissions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "News");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "News");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "News");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "News");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Approvers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Approvers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Approvers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Approvers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApprovalHistory");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ApprovalHistory");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ApprovalHistory");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ApprovalHistory");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "WorkItems",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "WorkItems",
                newName: "CreatedDate");
        }
    }
}
