namespace jobzilla_net.Application.Candidates.Queries;

/// <summary>
/// Query parameters for candidate search and listing.
/// All fields are optional — null or empty means no filter applied.
/// </summary>
public sealed class CandidateSearchQuery
{
    /// <summary>Free-text keyword matched against FullName and ProfessionalTitle.</summary>
    public string? Keyword { get; set; }

    /// <summary>Location text filter (partial match).</summary>
    public string? Location { get; set; }

    /// <summary>Minimum years of experience filter. Null = no filter.</summary>
    public int? MinExperienceYears { get; set; }

    /// <summary>Maximum expected salary filter (inclusive). Null = no filter.</summary>
    public decimal? MaxExpectedSalary { get; set; }

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
