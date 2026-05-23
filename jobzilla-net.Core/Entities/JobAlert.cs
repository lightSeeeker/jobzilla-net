using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class JobAlert : AuditableEntity
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public int? JobCategoryId { get; set; }
    public JobCategory? JobCategory { get; set; }
    public bool IsActive { get; set; } = true;
}
