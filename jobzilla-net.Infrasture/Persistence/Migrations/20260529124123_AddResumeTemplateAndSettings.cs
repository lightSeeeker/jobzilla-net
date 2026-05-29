using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeTemplateAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SettingsJson",
                table: "CandidateResumes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "CandidateResumes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateResumes_TemplateId",
                table: "CandidateResumes",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateResumes_ResumeTemplates_TemplateId",
                table: "CandidateResumes",
                column: "TemplateId",
                principalTable: "ResumeTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateResumes_ResumeTemplates_TemplateId",
                table: "CandidateResumes");

            migrationBuilder.DropIndex(
                name: "IX_CandidateResumes_TemplateId",
                table: "CandidateResumes");

            migrationBuilder.DropColumn(
                name: "SettingsJson",
                table: "CandidateResumes");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "CandidateResumes");
        }
    }
}
