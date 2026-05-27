using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class CandidateEducation : AuditableEntity
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }

    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentlyStudying { get; set; }
    public string? Description { get; set; }
}
