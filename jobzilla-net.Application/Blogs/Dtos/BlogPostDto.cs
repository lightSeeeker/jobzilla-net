namespace jobzilla_net.Application.Blogs.Dtos;

/// <summary>
/// Projection DTO for blog posts.
/// Populated exclusively by EF projection.
/// </summary>
public sealed class BlogPostDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? Content { get; init; } // Populated for detail view
    public string? FeaturedImagePath { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public string? AuthorName { get; init; }
    
    // Category info
    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }
}
