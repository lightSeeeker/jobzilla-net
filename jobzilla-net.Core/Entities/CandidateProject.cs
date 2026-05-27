using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class CandidateProject : AuditableEntity
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsOngoing { get; set; }
}
