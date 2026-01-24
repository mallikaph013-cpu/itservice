using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddSAPRegistrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BOICode",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseUnit",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostingLotSize",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistributionChannel",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishMatDescription",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ICSCode",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFG",
                table: "SupportRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRM",
                table: "SupportRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSM",
                table: "SupportRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MRPController",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MakerMfrPartNumber",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialGroup",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductionSupervisor",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfitCenter",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLoc",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLocB1",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLocBP",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValClass",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BOICode",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "BaseUnit",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "CostingLotSize",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "DistributionChannel",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "EnglishMatDescription",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "ICSCode",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "IsFG",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "IsRM",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "IsSM",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "MRPController",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "MakerMfrPartNumber",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "MaterialGroup",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "Plant",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "ProductionSupervisor",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "ProfitCenter",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "StorageLoc",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "StorageLocB1",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "StorageLocBP",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "ValClass",
                table: "SupportRequests");
        }
    }
}
