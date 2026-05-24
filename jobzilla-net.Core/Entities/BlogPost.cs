using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class BlogPost : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? FeaturedImagePath { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    
    public string? AuthorName { get; set; }
    
    public int? CategoryId { get; set; }
    public BlogCategory? Category { get; set; }
}
