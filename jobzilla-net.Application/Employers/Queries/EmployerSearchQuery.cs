namespace jobzilla_net.Application.Employers.Queries;

/// <summary>
/// Query parameters for employer search and listing.
/// All fields are optional — null or empty means no filter applied.
/// </summary>
public sealed class EmployerSearchQuery
{
    /// <summary>Free-text keyword matched against CompanyName.</summary>
    public string? Keyword { get; set; }

    /// <summary>Location text filter (partial match).</summary>
    public string? Location { get; set; }

    /// <summary>Industry filter (exact or partial match).</summary>
    public string? Industry { get; set; }

    /// <summary>Company size filter.</summary>
    public string? CompanySize { get; set; }

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
