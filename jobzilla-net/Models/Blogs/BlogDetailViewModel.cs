using jobzilla_net.Application.Blogs.Dtos;
using jobzilla_net.Core.Entities;

namespace jobzilla_net.Models.Blogs;

/// <summary>
/// View model for the public blog detail page.
/// Carries the post detail and sidebar data.
/// </summary>
public sealed class BlogDetailViewModel
{
    public BlogPostDto Post { get; set; } = null!;

    public IReadOnlyList<BlogCategory> Categories { get; set; } = Array.Empty<BlogCategory>();

    public IReadOnlyList<BlogPostDto> LatestPosts { get; set; } = Array.Empty<BlogPostDto>();
}
