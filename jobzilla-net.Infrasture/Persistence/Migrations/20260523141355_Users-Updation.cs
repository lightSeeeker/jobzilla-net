using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UsersUpdation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateSkill_CandidateProfiles_CandidateProfileId",
                table: "CandidateSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidateSkill_Skills_SkillId",
                table: "CandidateSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPostSkill_JobPosts_JobPostId",
                table: "JobPostSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPostSkill_Skills_SkillId",
                table: "JobPostSkill");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobPostSkill",
                table: "JobPostSkill");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CandidateSkill",
                table: "CandidateSkill");

            migrationBuilder.RenameTable(
                name: "JobPostSkill",
                newName: "JobPostSkills");

            migrationBuilder.RenameTable(
                name: "CandidateSkill",
                newName: "CandidateSkills");

            migrationBuilder.RenameIndex(
                name: "IX_JobPostSkill_SkillId",
                table: "JobPostSkills",
                newName: "IX_JobPostSkills_SkillId");

            migrationBuilder.RenameIndex(
                name: "IX_CandidateSkill_SkillId",
                table: "CandidateSkills",
                newName: "IX_CandidateSkills_SkillId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobPostSkills",
                table: "JobPostSkills",
                columns: new[] { "JobPostId", "SkillId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CandidateSkills",
                table: "CandidateSkills",
                columns: new[] { "CandidateProfileId", "SkillId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateSkills_CandidateProfiles_CandidateProfileId",
                table: "CandidateSkills",
                column: "CandidateProfileId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateSkills_Skills_SkillId",
                table: "CandidateSkills",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostSkills_JobPosts_JobPostId",
                table: "JobPostSkills",
                column: "JobPostId",
                principalTable: "JobPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostSkills_Skills_SkillId",
                table: "JobPostSkills",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateSkills_CandidateProfiles_CandidateProfileId",
                table: "CandidateSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidateSkills_Skills_SkillId",
                table: "CandidateSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPostSkills_JobPosts_JobPostId",
                table: "JobPostSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_JobPostSkills_Skills_SkillId",
                table: "JobPostSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobPostSkills",
                table: "JobPostSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CandidateSkills",
                table: "CandidateSkills");

            migrationBuilder.RenameTable(
                name: "JobPostSkills",
                newName: "JobPostSkill");

            migrationBuilder.RenameTable(
                name: "CandidateSkills",
                newName: "CandidateSkill");

            migrationBuilder.RenameIndex(
                name: "IX_JobPostSkills_SkillId",
                table: "JobPostSkill",
                newName: "IX_JobPostSkill_SkillId");

            migrationBuilder.RenameIndex(
                name: "IX_CandidateSkills_SkillId",
                table: "CandidateSkill",
                newName: "IX_CandidateSkill_SkillId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobPostSkill",
                table: "JobPostSkill",
                columns: new[] { "JobPostId", "SkillId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CandidateSkill",
                table: "CandidateSkill",
                columns: new[] { "CandidateProfileId", "SkillId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateSkill_CandidateProfiles_CandidateProfileId",
                table: "CandidateSkill",
                column: "CandidateProfileId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateSkill_Skills_SkillId",
                table: "CandidateSkill",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostSkill_JobPosts_JobPostId",
                table: "JobPostSkill",
                column: "JobPostId",
                principalTable: "JobPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostSkill_Skills_SkillId",
                table: "JobPostSkill",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
