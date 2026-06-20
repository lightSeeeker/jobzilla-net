using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPanelEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ResumeTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPrice",
                table: "ResumeTemplates",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "ResumeTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ResumeTemplates",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TemplateType",
                table: "ResumeTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAuditLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "DiscountPrice", "IsPremium", "Price", "TemplateType" },
                values: new object[] { "Professional", null, false, 0m, "Modern" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "DiscountPrice", "IsPremium", "Price", "TemplateType" },
                values: new object[] { "Executive", null, true, 29.99m, "Corporate" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "DiscountPrice", "IsPremium", "Price", "TemplateType" },
                values: new object[] { "ATS Friendly", null, true, 19.99m, "ATS" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "DiscountPrice", "IsPremium", "Price", "TemplateType" },
                values: new object[] { "Creative", null, true, 39.99m, "Creative" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "DiscountPrice", "IsPremium", "Price", "TemplateType" },
                values: new object[] { "Technical", null, true, 24.99m, "Technical" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAuditLogs");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ResumeTemplates");

            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "ResumeTemplates");

            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "ResumeTemplates");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ResumeTemplates");

            migrationBuilder.DropColumn(
                name: "TemplateType",
                table: "ResumeTemplates");
        }
    }
}
