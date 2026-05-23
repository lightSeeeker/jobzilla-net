using jobzilla_net.Core.Common;
using jobzilla_net.Core.Enums;

namespace jobzilla_net.Core.Entities;

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

    public Conversation? Conversation { get; set; }
}
