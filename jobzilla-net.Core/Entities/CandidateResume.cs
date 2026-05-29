using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class CandidateResume : AuditableEntity
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? DocumentData { get; set; }
    public bool IsBuilderGenerated { get; set; }
}
