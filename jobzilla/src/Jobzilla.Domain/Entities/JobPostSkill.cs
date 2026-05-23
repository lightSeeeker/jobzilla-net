namespace Jobzilla.Domain.Entities;

public class JobPostSkill
{
    public int JobPostId { get; set; }
    public JobPost? JobPost { get; set; }
    public int SkillId { get; set; }
    public Skill? Skill { get; set; }
}
