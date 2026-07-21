using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeTemplateSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "ResumeTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "Source",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "Source",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "Source",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 4,
                column: "Source",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ResumeTemplates",
                keyColumn: "Id",
                keyValue: 5,
                column: "Source",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "ResumeTemplates");
        }
    }
}
