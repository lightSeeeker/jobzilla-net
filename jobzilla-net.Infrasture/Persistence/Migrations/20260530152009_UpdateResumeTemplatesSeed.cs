using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResumeTemplatesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "TemplateFilePath" },
                values: new object[] { "A clean, balanced layout with subtle colors suitable for modern professionals.", "Modern Professional", "ModernProfessional" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "TemplateFilePath" },
                values: new object[] { "Traditional and sophisticated, perfect for senior corporate roles.", "Executive Corporate", "ExecutiveCorporate" });

            migrationBuilder.InsertData(
                table: "ResumeTemplates",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "IsActive", "IsDeleted", "Name", "PreviewImagePath", "TemplateFilePath", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Strictly single column, text-focused format designed to pass cleanly through tracking systems.", true, false, "ATS Optimized", null, "AtsOptimized", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bold typography and vibrant accents for creative and design-focused roles.", true, false, "Creative Designer", null, "CreativeDesigner", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Clean and structured with tech-focused elements, resembling technical documentation.", true, false, "Technical Developer", null, "TechnicalDeveloper", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "TemplateFilePath" },
                values: new object[] { "A clean, professional template suitable for all industries.", "Standard Professional", "Standard" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "TemplateFilePath" },
                values: new object[] { "A sleek, modern design with vibrant accents for creative roles.", "Modern Creative", "Modern" });
        }
    }
}
