using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class JobCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconCssClass { get; set; }
    public string? Description { get; set; }

    public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
}
