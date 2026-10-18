using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChugimCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Enrollments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Enrollments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaidById",
                table: "Enrollments",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsPaid", "PaidAt", "PaidById" },
                values: new object[] { false, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_PaidById",
                table: "Enrollments",
                column: "PaidById");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Users_PaidById",
                table: "Enrollments",
                column: "PaidById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Users_PaidById",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_PaidById",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "PaidById",
                table: "Enrollments");
        }
    }
}
