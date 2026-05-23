namespace Jobzilla.Domain.Entities;

public class SavedJob
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }
    public int JobPostId { get; set; }
    public JobPost? JobPost { get; set; }
    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
}
