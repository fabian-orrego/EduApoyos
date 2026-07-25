using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduApoyos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupportType = table.Column<int>(type: "int", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdvisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportRequests", x => x.Id);
                    table.CheckConstraint("CK_SupportRequests_RequestedAmount_Positive", "[RequestedAmount] > 0");
                    table.ForeignKey(
                        name: "FK_SupportRequests_AspNetUsers_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportRequests_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_AdvisorId",
                table: "SupportRequests",
                column: "AdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_Status",
                table: "SupportRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_StudentId",
                table: "SupportRequests",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportRequests");
        }
    }
}
