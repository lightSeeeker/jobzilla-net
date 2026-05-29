using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class CandidateReference : AuditableEntity
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }

    public string ReferenceName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Designation { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Relationship { get; set; }
    public string? Notes { get; set; }
    public int DisplayOrder { get; set; }
}
