using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RebuildResumeTemplatesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safety: re-point any resume referencing a template outside the new 1-5 range to the
            // default template so the FK stays valid (no-op on a fresh DB with only Ids 1-5).
            migrationBuilder.Sql("UPDATE CandidateResumes SET TemplateId = 1 WHERE TemplateId IS NOT NULL AND TemplateId NOT IN (1, 2, 3, 4, 5);");

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "ATS Friendly", "Single-column, graphics-free layout built to pass cleanly through applicant tracking systems.", false, "ATS Minimal", 0m, "AtsMinimal", "ATS" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "Professional", "Clean single-column resume with a solid accent header — a safe, polished all-rounder.", false, "Professional", 0m, "ProfessionalClean", "Professional" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "Professional", "Two-column layout with a tinted sidebar for contact, skills and languages beside your experience.", false, "Classic Two-Column", 0m, "ClassicTwoColumn", "TwoColumn" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "Modern", "Contemporary single-column design with accent headings and a subtle timeline accent bar.", false, "Modern", 0m, "ModernAccent", "Modern" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "Professional", "Two-column layout with a full-height dark sidebar and blue accents for a corporate, executive feel.", false, "Corporate", 0m, "CorporateBlue", "TwoColumn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "Professional", "A clean, balanced layout with subtle colors suitable for modern professionals.", false, "Modern Professional", 0m, "ModernProfessional", "Modern" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "Executive", "Traditional and sophisticated, perfect for senior corporate roles.", true, "Executive Corporate", 29.99m, "ExecutiveCorporate", "Corporate" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "ATS Friendly", "Strictly single column, text-focused format designed to pass cleanly through tracking systems.", true, "ATS Optimized", 19.99m, "AtsOptimized", "ATS" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "Creative", "Bold typography and vibrant accents for creative and design-focused roles.", true, "Creative Designer", 39.99m, "CreativeDesigner", "Creative" });

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Description", "IsPremium", "Name", "Price", "TemplateFilePath", "TemplateType" },
                values: new object[] { "Technical", "Clean and structured with tech-focused elements, resembling technical documentation.", true, "Technical Developer", 24.99m, "TechnicalDeveloper", "Technical" });
        }
    }
}
