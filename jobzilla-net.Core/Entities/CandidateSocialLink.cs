using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class CandidateSocialLink : AuditableEntity
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }

    public string PlatformName { get; set; } = string.Empty; // e.g., LinkedIn, GitHub, Portfolio
    public string Url { get; set; } = string.Empty;
}
