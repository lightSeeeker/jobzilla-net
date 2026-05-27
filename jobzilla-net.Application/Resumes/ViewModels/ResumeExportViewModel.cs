using jobzilla_net.Application.Candidates.Dtos;

namespace jobzilla_net.Application.Resumes.ViewModels;

public class ResumeExportViewModel
{
    // Basic personal information (reuse existing DTO or define specific fields)
    public CandidateProfileDto Profile { get; set; } = new();

    // Grouped resume sections
    public List<CandidateExperienceViewModel> Experiences { get; set; } = new();
    public List<CandidateEducationViewModel> Educations { get; set; } = new();
    public List<CandidateCertificationViewModel> Certifications { get; set; } = new();
    public List<CandidateProjectViewModel> Projects { get; set; } = new();
    public List<CandidateSocialLinkViewModel> SocialLinks { get; set; } = new();
    
    // Skills can be simple strings or objects if level/category is needed
    public List<string> Skills { get; set; } = new();
}
