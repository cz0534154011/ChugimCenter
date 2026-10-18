using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChugimCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ParentName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ParentPhone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    InstructorId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SessionsCount = table.Column<int>(type: "integer", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    MinAge = table.Column<int>(type: "integer", nullable: false),
                    MaxAge = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    EnrolledCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGroups", x => x.Id);
                    table.CheckConstraint("CK_CourseGroups_EnrolledCount", "\"EnrolledCount\" >= 0 AND \"EnrolledCount\" <= \"Capacity\"");
                    table.ForeignKey(
                        name: "FK_CourseGroups_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseGroups_Users_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CourseGroupId = table.Column<int>(type: "integer", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledById = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_CourseGroups_CourseGroupId",
                        column: x => x.CourseGroupId,
                        principalTable: "CourseGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Users_CancelledById",
                        column: x => x.CancelledById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, true, "ציור" },
                    { 2, true, "דרמה" },
                    { 3, true, "התעמלות" },
                    { 4, true, "מוזיקה" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "FullName", "ParentName", "ParentPhone", "PasswordHash", "Phone", "Role" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@example.com", "רבקה לוי", null, null, "$2a$11$nNI43eulvTnlj41G1OvBwu029P..M.PiRacBxBeQlYRub2y73dFbu", null, 1 },
                    { 2, null, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sara@example.com", "שרה כהן", null, null, "$2a$11$cSqSbGB5hx4kB36WlkVNM.sFeleoz4li5uM3xasATQMvEKzRL/QQq", "050-1111111", 2 },
                    { 3, null, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "miriam@example.com", "מרים פרידמן", null, null, "$2a$11$cSqSbGB5hx4kB36WlkVNM.sFeleoz4li5uM3xasATQMvEKzRL/QQq", "052-2222222", 2 },
                    { 4, new DateOnly(2017, 3, 15), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "chana@example.com", "חנה גולד", "יעל גולד", "054-3333333", "$2a$11$8iUOwsheAj22pCaXjSc93eKJMzxcYLwWBGZ1YjGdvfGyJMXJgI.6u", null, 3 },
                    { 5, new DateOnly(2015, 6, 20), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "esther@example.com", "אסתר רוזן", "דבורה רוזן", "053-4444444", "$2a$11$8iUOwsheAj22pCaXjSc93eKJMzxcYLwWBGZ1YjGdvfGyJMXJgI.6u", null, 3 },
                    { 6, new DateOnly(2016, 1, 10), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "leah@example.com", "לאה שטרן", "רחל שטרן", "058-5555555", "$2a$11$8iUOwsheAj22pCaXjSc93eKJMzxcYLwWBGZ1YjGdvfGyJMXJgI.6u", null, 3 }
                });

            migrationBuilder.InsertData(
                table: "CourseGroups",
                columns: new[] { "Id", "Capacity", "CategoryId", "DayOfWeek", "DurationMinutes", "EnrolledCount", "InstructorId", "IsActive", "MaxAge", "MinAge", "Name", "Price", "SessionsCount", "StartDate", "StartTime", "Version" },
                values: new object[,]
                {
                    { 1, 12, 1, 0, 60, 0, 2, true, 9, 6, "ציור מתחילות", 180m, 30, new DateOnly(2026, 11, 1), new TimeOnly(16, 0, 0), new Guid("0a1b2c3d-0001-4000-8000-000000000001") },
                    { 2, 10, 1, 2, 75, 0, 2, true, 13, 10, "ציור מתקדמות", 200m, 30, new DateOnly(2026, 11, 3), new TimeOnly(17, 0, 0), new Guid("0a1b2c3d-0002-4000-8000-000000000002") },
                    { 3, 2, 2, 1, 60, 1, 3, true, 11, 8, "דרמה גילאי 8–11", 190m, 30, new DateOnly(2026, 11, 2), new TimeOnly(16, 30, 0), new Guid("0a1b2c3d-0003-4000-8000-000000000003") },
                    { 4, 15, 3, 3, 45, 0, 3, true, 10, 6, "התעמלות קרקע", 170m, 30, new DateOnly(2026, 11, 4), new TimeOnly(16, 0, 0), new Guid("0a1b2c3d-0004-4000-8000-000000000004") },
                    { 5, 20, 4, 4, 60, 0, 2, true, 14, 8, "מקהלת בנות", 160m, 30, new DateOnly(2026, 11, 5), new TimeOnly(17, 30, 0), new Guid("0a1b2c3d-0005-4000-8000-000000000005") }
                });

            migrationBuilder.InsertData(
                table: "Enrollments",
                columns: new[] { "Id", "CancelledAt", "CancelledById", "CourseGroupId", "EnrolledAt", "Status", "UserId" },
                values: new object[] { 1, null, null, 3, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4 });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroups_CategoryId",
                table: "CourseGroups",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroups_InstructorId",
                table: "CourseGroups",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CancelledById",
                table: "Enrollments",
                column: "CancelledById");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseGroupId",
                table: "Enrollments",
                column: "CourseGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_UserId_CourseGroupId",
                table: "Enrollments",
                columns: new[] { "UserId", "CourseGroupId" },
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "CourseGroups");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
