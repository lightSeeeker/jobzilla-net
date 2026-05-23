namespace jobzilla_net.Core.Entities;

public class CandidateSkill
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }
    public int SkillId { get; set; }
    public Skill? Skill { get; set; }
}
