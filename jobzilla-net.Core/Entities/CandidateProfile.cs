using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class CandidateProfile : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfessionalTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string? Summary { get; set; }
    public string? ProfileImagePath { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? ExpectedSalary { get; set; }

    public ICollection<CandidateResume> Resumes { get; set; } = new List<CandidateResume>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public ICollection<JobAlert> JobAlerts { get; set; } = new List<JobAlert>();
    public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
    public ICollection<CandidateExperience> Experiences { get; set; } = new List<CandidateExperience>();
    public ICollection<CandidateEducation> Educations { get; set; } = new List<CandidateEducation>();
    public ICollection<CandidateCertification> Certifications { get; set; } = new List<CandidateCertification>();
    public ICollection<CandidateProject> Projects { get; set; } = new List<CandidateProject>();
    public ICollection<CandidateSocialLink> SocialLinks { get; set; } = new List<CandidateSocialLink>();
    public ICollection<CandidateReference> References { get; set; } = new List<CandidateReference>();
}
