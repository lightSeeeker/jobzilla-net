using jobzilla_net.Application.Blogs.Dtos;
using jobzilla_net.Application.Blogs.Queries;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Application.Blogs;

/// <summary>
/// Application-layer contract for blog-related read operations.
/// </summary>
public interface IBlogService
{
    /// <summary>
    /// Returns a paginated, filtered list of published blog posts.
    /// Content is typically excluded or truncated in this projection.
    /// </summary>
    Task<PagedResult<BlogPostDto>> SearchAsync(
        BlogSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single published blog post by its Slug, including full content.
    /// Returns <c>null</c> if not found or not published.
    /// </summary>
    Task<BlogPostDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the latest published blog posts for sidebars/widgets.
    /// </summary>
    Task<IReadOnlyList<BlogPostDto>> GetLatestPostsAsync(
        int count = 3,
        CancellationToken cancellationToken = default);
}
