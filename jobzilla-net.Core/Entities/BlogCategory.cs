using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class BlogCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    
    // Navigation
    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
}
