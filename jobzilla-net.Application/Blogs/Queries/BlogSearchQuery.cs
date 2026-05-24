namespace jobzilla_net.Application.Blogs.Queries;

/// <summary>
/// Query parameters for blog search and listing.
/// </summary>
public sealed class BlogSearchQuery
{
    /// <summary>Free-text keyword matched against Title and Excerpt.</summary>
    public string? Keyword { get; set; }

    /// <summary>Optional category filter. Null = all categories.</summary>
    public int? CategoryId { get; set; }

    // ── Pagination ───────────────────────────────────────────────────────────

    private int _page = 1;
    private int _pageSize = 10;

    /// <summary>1-based page number. Defaults to 1; clamped to ≥1.</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Items per page. Defaults to 10; clamped to 1–100.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : value > 100 ? 100 : value;
    }
}
