using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduApoyos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportRequestsStatusUpdatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupportRequests_Status",
                table: "SupportRequests");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_Status_UpdatedAt",
                table: "SupportRequests",
                columns: new[] { "Status", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupportRequests_Status_UpdatedAt",
                table: "SupportRequests");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_Status",
                table: "SupportRequests",
                column: "Status");
        }
    }
}
