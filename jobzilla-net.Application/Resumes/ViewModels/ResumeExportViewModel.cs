using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Resumes.Dtos;

namespace jobzilla_net.Application.Resumes.ViewModels;

public class ResumeExportViewModel
{
    // Basic personal information (reuse existing DTO or define specific fields)
    public CandidateProfileDto Profile { get; set; } = new();

    // Grouped resume sections (legacy typed collections — kept for template backward-compat)
    public List<CandidateExperienceViewModel> Experiences { get; set; } = new();
    public List<CandidateEducationViewModel> Educations { get; set; } = new();
    public List<CandidateCertificationViewModel> Certifications { get; set; } = new();
    public List<CandidateProjectViewModel> Projects { get; set; } = new();
    public List<CandidateSocialLinkViewModel> SocialLinks { get; set; } = new();
    public List<string> Skills { get; set; } = new();

    // ── Dynamic Builder Document ───────────────────────────────────────────────
    // Populated when the Builder is active. Templates prefer this over the typed
    // collections above when it is not null, enabling fully dynamic section rendering.
    public ResumeDocument? Document { get; set; }

    // References (new — stored separately from the parsed profile data)
    public List<ReferenceViewModel> References { get; set; } = new();

    // ── Rendering Context ──────────────────────────────────────────────────
    // Added to support dynamic UI controls (download buttons) vs PDF export.
    public int? TemplateId { get; set; }
    public int? ResumeId { get; set; }
    public bool IsExport { get; set; }

    // ── Unified Rendering Helper ───────────────────────────────────────────
    // Provides a single collection of ResumeSections regardless of whether the
    // resume was built with the dynamic builder or from legacy typed data.
    public IEnumerable<ResumeSection> GetUnifiedSections()
    {
        if (Document != null && Document.Sections.Any())
        {
            return Document.Sections.OrderBy(s => s.DisplayOrder);
        }

        var sections = new List<ResumeSection>();
        int order = 1;

        if (Experiences.Any())
        {
            var sec = new ResumeSection { SectionType = ResumeBuilderSectionType.Experience, Title = "Experience", DisplayOrder = order++ };
            int i = 1;
            foreach (var e in Experiences.OrderByDescending(x => x.StartDate))
            {
                var item = new ResumeSectionItem { DisplayOrder = i++ };
                item.SetValue("JobTitle", e.JobTitle);
                item.SetValue("CompanyName", e.CompanyName);
                item.SetValue("Location", e.Location);
                item.SetValue("StartDate", e.StartDate.ToString("O"));
                item.SetValue("EndDate", e.EndDate?.ToString("O"));
                item.SetValue("IsCurrent", e.EndDate == null ? "true" : "false");
                item.SetValue("Description", e.Description);
                sec.Items.Add(item);
            }
            sections.Add(sec);
        }

        if (Educations.Any())
        {
            var sec = new ResumeSection { SectionType = ResumeBuilderSectionType.Education, Title = "Education", DisplayOrder = order++ };
            int i = 1;
            foreach (var e in Educations.OrderByDescending(x => x.StartDate))
            {
                var item = new ResumeSectionItem { DisplayOrder = i++ };
                item.SetValue("InstitutionName", e.InstitutionName);
                item.SetValue("Degree", e.Degree);
                item.SetValue("FieldOfStudy", e.FieldOfStudy);
                item.SetValue("StartDate", e.StartDate.ToString("O"));
                item.SetValue("EndDate", e.EndDate?.ToString("O"));
                item.SetValue("IsCurrent", e.EndDate == null ? "true" : "false");
                sec.Items.Add(item);
            }
            sections.Add(sec);
        }

        if (Certifications.Any())
        {
            var sec = new ResumeSection { SectionType = ResumeBuilderSectionType.Certifications, Title = "Certifications", DisplayOrder = order++ };
            int i = 1;
            foreach (var c in Certifications.OrderByDescending(x => x.IssueDate))
            {
                var item = new ResumeSectionItem { DisplayOrder = i++ };
                item.SetValue("Name", c.Name);
                item.SetValue("IssuingOrganization", c.IssuingOrganization);
                item.SetValue("IssueDate", c.IssueDate.ToString("O"));
                item.SetValue("CredentialUrl", c.CredentialUrl);
                sec.Items.Add(item);
            }
            sections.Add(sec);
        }

        if (Projects.Any())
        {
            var sec = new ResumeSection { SectionType = ResumeBuilderSectionType.Projects, Title = "Projects", DisplayOrder = order++ };
            int i = 1;
            foreach (var p in Projects.OrderByDescending(x => x.StartDate))
            {
                var item = new ResumeSectionItem { DisplayOrder = i++ };
                item.SetValue("Name", p.Name);
                item.SetValue("Description", p.Description);
                item.SetValue("ProjectUrl", p.ProjectUrl);
                item.SetValue("StartDate", p.StartDate?.ToString("O"));
                item.SetValue("EndDate", p.EndDate?.ToString("O"));
                sec.Items.Add(item);
            }
            sections.Add(sec);
        }

        if (Skills.Any())
        {
            var sec = new ResumeSection { SectionType = ResumeBuilderSectionType.Skills, Title = "Skills", DisplayOrder = order++ };
            int i = 1;
            foreach (var s in Skills)
            {
                var item = new ResumeSectionItem { DisplayOrder = i++ };
                item.SetValue("Name", s);
                sec.Items.Add(item);
            }
            sections.Add(sec);
        }

        if (References.Any())
        {
            var sec = new ResumeSection { SectionType = ResumeBuilderSectionType.References, Title = "References", DisplayOrder = order++ };
            int i = 1;
            foreach (var r in References)
            {
                var item = new ResumeSectionItem { DisplayOrder = i++ };
                item.SetValue("ReferenceName", r.ReferenceName);
                item.SetValue("Designation", r.Designation);
                item.SetValue("Company", r.Company);
                item.SetValue("Phone", r.Phone);
                item.SetValue("Email", r.Email);
                sec.Items.Add(item);
            }
            sections.Add(sec);
        }

        return sections;
    }
}

