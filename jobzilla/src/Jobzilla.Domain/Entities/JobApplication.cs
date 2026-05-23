using Jobzilla.Domain.Common;
using Jobzilla.Domain.Enums;

namespace Jobzilla.Domain.Entities;

public class JobApplication : AuditableEntity
{
    public int JobPostId { get; set; }
    public JobPost? JobPost { get; set; }
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }
    public int? CandidateResumeId { get; set; }
    public CandidateResume? CandidateResume { get; set; }
    public string? CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;
}
