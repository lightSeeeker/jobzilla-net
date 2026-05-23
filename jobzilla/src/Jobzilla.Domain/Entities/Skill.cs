using Jobzilla.Domain.Common;

namespace Jobzilla.Domain.Entities;

public class Skill : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    public ICollection<JobPostSkill> JobPostSkills { get; set; } = new List<JobPostSkill>();
}
