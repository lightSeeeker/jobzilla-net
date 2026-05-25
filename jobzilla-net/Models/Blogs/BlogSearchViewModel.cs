using jobzilla_net.Application.Blogs.Dtos;
using jobzilla_net.Application.Blogs.Queries;
using jobzilla_net.Application.Common;
using jobzilla_net.Core.Entities;

namespace jobzilla_net.Models.Blogs;

/// <summary>
/// View model for the public blog listing page (Blogs/Index).
/// Binds query-string filters from the GET request and carries
/// the paged result back to the Razor view, along with categories and latest posts for the sidebar.
/// </summary>
public sealed class BlogSearchViewModel
{
    // ── Filter inputs (bound from query string) ───────────────────────────────

    /// <summary>Free-text keyword (title, excerpt).</summary>
    public string? Keyword { get; set; }

    /// <summary>Category filter.</summary>
    public int? CategoryId { get; set; }

    /// <summary>Current page number. Defaults to 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Items per page. Defaults to 10.</summary>
    public int PageSize { get; set; } = 10;

    // ── Result data (populated by controller) ─────────────────────────────────

    /// <summary>Paged blog results returned from the service.</summary>
    public PagedResult<BlogPostDto> Result { get; set; } =
        PagedResult<BlogPostDto>.Empty(1, 10);

    /// <summary>
    /// All active categories for the filter sidebar.
    /// Populated by the controller from the database.
    /// </summary>
    public IReadOnlyList<BlogCategory> Categories { get; set; } = Array.Empty<BlogCategory>();

    /// <summary>
    /// Latest posts for the sidebar widget.
    /// </summary>
    public IReadOnlyList<BlogPostDto> LatestPosts { get; set; } = Array.Empty<BlogPostDto>();

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>True when any filter is actively applied.</summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Keyword) ||
        CategoryId.HasValue;

    /// <summary>Converts this view model into an Application-layer query.</summary>
    public BlogSearchQuery ToQuery() => new()
    {
        Keyword    = Keyword,
        CategoryId = CategoryId,
        Page       = Page,
        PageSize   = PageSize
    };
}
