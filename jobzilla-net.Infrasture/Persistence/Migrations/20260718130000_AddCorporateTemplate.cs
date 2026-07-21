using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jobzilla_net.Infrasture.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Corrective, idempotent seed for the Corporate (Id 5) template. An earlier revision of
            // RebuildResumeTemplatesSeed deleted Id 5; since EF keys migrations by id it will not
            // re-run, so re-add the row here (insert if missing, else update).
            migrationBuilder.Sql(@"
SET IDENTITY_INSERT ResumeTemplates ON;
IF NOT EXISTS (SELECT 1 FROM ResumeTemplates WHERE Id = 5)
    INSERT INTO ResumeTemplates (Id, Name, Description, TemplateFilePath, PreviewImagePath, IsActive, IsDeleted, IsPremium, Price, DiscountPrice, Category, TemplateType, Source, CreatedAtUtc, UpdatedAtUtc)
    VALUES (5, N'Corporate', N'Two-column layout with a full-height dark sidebar and blue accents for a corporate, executive feel.', N'CorporateBlue', NULL, 1, 0, 0, 0, NULL, N'Professional', N'TwoColumn', 0, '2026-01-01T00:00:00', NULL);
ELSE
    UPDATE ResumeTemplates
       SET Name = N'Corporate',
           Description = N'Two-column layout with a full-height dark sidebar and blue accents for a corporate, executive feel.',
           TemplateFilePath = N'CorporateBlue',
           IsActive = 1,
           IsDeleted = 0,
           IsPremium = 0,
           Price = 0,
           Category = N'Professional',
           TemplateType = N'TwoColumn',
           Source = 0
     WHERE Id = 5;
SET IDENTITY_INSERT ResumeTemplates OFF;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE CandidateResumes SET TemplateId = 1 WHERE TemplateId = 5;
DELETE FROM ResumeTemplates WHERE Id = 5;
");
        }
    }
}
