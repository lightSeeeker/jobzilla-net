using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanySizeToEmployerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanySize",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EmployerProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CompanySize",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanySize",
                table: "EmployerProfiles");
        }
    }
}
