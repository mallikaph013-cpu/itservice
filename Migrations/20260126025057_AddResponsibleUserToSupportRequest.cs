using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddResponsibleUserToSupportRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponsibleUserId",
                table: "SupportRequests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_CurrentApproverId",
                table: "SupportRequests",
                column: "CurrentApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_ResponsibleUserId",
                table: "SupportRequests",
                column: "ResponsibleUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportRequests_Approvers_CurrentApproverId",
                table: "SupportRequests",
                column: "CurrentApproverId",
                principalTable: "Approvers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportRequests_Users_ResponsibleUserId",
                table: "SupportRequests",
                column: "ResponsibleUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportRequests_Approvers_CurrentApproverId",
                table: "SupportRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportRequests_Users_ResponsibleUserId",
                table: "SupportRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupportRequests_CurrentApproverId",
                table: "SupportRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupportRequests_ResponsibleUserId",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                table: "SupportRequests");
        }
    }
}
