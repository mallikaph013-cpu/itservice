using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTypeToApprovalSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalHistory_SupportRequests_SupportRequestId",
                table: "ApprovalHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalHistory",
                table: "ApprovalHistory");

            migrationBuilder.RenameTable(
                name: "ApprovalHistory",
                newName: "ApprovalHistories");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalHistory_SupportRequestId",
                table: "ApprovalHistories",
                newName: "IX_ApprovalHistories_SupportRequestId");

            migrationBuilder.AddColumn<int>(
                name: "RequestType",
                table: "ApprovalSequences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalHistories",
                table: "ApprovalHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalHistories_SupportRequests_SupportRequestId",
                table: "ApprovalHistories",
                column: "SupportRequestId",
                principalTable: "SupportRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalHistories_SupportRequests_SupportRequestId",
                table: "ApprovalHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApprovalHistories",
                table: "ApprovalHistories");

            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "ApprovalSequences");

            migrationBuilder.RenameTable(
                name: "ApprovalHistories",
                newName: "ApprovalHistory");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalHistories_SupportRequestId",
                table: "ApprovalHistory",
                newName: "IX_ApprovalHistory_SupportRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApprovalHistory",
                table: "ApprovalHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalHistory_SupportRequests_SupportRequestId",
                table: "ApprovalHistory",
                column: "SupportRequestId",
                principalTable: "SupportRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
