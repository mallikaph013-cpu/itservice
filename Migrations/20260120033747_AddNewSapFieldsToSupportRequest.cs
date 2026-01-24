using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddNewSapFieldsToSupportRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BOIDescription",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommCodeTariffCode",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceControl",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchasingGroup",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TariffCodePercentage",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BOIDescription",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "CommCodeTariffCode",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "PriceControl",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "PurchasingGroup",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "TariffCodePercentage",
                table: "SupportRequests");
        }
    }
}
