using Jobzilla.Domain.Common;

namespace Jobzilla.Domain.Entities;

public class JobCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconCssClass { get; set; }
    public string? Description { get; set; }
    public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
}
