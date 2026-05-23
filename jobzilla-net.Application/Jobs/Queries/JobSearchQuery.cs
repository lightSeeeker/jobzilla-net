namespace jobzilla_net.Application.Jobs.Queries;

/// <summary>
/// Search/filter parameters for the public job listing.
/// All filters are optional — omitting them returns all published jobs.
/// Designed to be bound directly from the HTTP query string.
/// </summary>
public sealed class JobSearchQuery
{
    // ── Filters ──────────────────────────────────────────────────────────────

    /// <summary>Matches against job title, description, and company name.</summary>
    public string? Keyword { get; set; }

    /// <summary>Matches against job location (case-insensitive contains).</summary>
    public string? Location { get; set; }

    /// <summary>Filters by <see cref="jobzilla_net.Core.Entities.JobCategory"/> Id.</summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// String representation of <see cref="jobzilla_net.Core.Enums.EmploymentType"/>
    /// (e.g. "FullTime", "Contract"). Null = no filter.
    /// </summary>
    public string? EmploymentType { get; set; }

    /// <summary>Minimum salary filter (inclusive). Null = no filter.</summary>
    public decimal? MinSalary { get; set; }

    /// <summary>Maximum salary filter (inclusive). Null = no filter.</summary>
    public decimal? MaxSalary { get; set; }

    // ── Pagination ───────────────────────────────────────────────────────────

    private int _page = 1;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 10;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > 50 ? 10 : value;
    }
}
