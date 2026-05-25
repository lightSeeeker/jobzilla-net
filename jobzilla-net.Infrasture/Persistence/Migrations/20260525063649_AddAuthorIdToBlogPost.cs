using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorIdToBlogPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "BlogPosts");
        }
    }
}
