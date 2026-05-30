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
}

