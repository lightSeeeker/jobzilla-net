using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedChatFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentName",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAtUtc",
                table: "Messages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedByCandidate",
                table: "Conversations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedByEmployer",
                table: "Conversations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentName",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "EditedAtUtc",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDeletedByCandidate",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IsDeletedByEmployer",
                table: "Conversations");
        }
    }
}
