using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Candidates.Queries;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Models.Candidates;

/// <summary>
/// View model for the public candidate listing page (Candidates/Index).
/// Binds query-string filters from the GET request and carries
/// the paged result back to the Razor view.
/// </summary>
public sealed class CandidateSearchViewModel
{
    // ── Filter inputs (bound from query string) ───────────────────────────────

    /// <summary>Free-text keyword (name, professional title).</summary>
    public string? Keyword { get; set; }

    /// <summary>Location text filter.</summary>
    public string? Location { get; set; }

    /// <summary>Minimum years of experience filter. Null = no filter.</summary>
    public int? MinExperienceYears { get; set; }

    /// <summary>Maximum expected salary filter. Null = no filter.</summary>
    public decimal? MaxExpectedSalary { get; set; }

    /// <summary>Current page number. Defaults to 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Items per page. Defaults to 10.</summary>
    public int PageSize { get; set; } = 10;

    // ── Result data (populated by controller) ─────────────────────────────────

    /// <summary>Paged candidate results returned from the service.</summary>
    public PagedResult<CandidateSummaryDto> Result { get; set; } =
        PagedResult<CandidateSummaryDto>.Empty(1, 10);

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>True when any filter is actively applied.</summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Keyword) ||
        !string.IsNullOrWhiteSpace(Location) ||
        MinExperienceYears.HasValue ||
        MaxExpectedSalary.HasValue;

    /// <summary>Converts this view model into an Application-layer query.</summary>
    public CandidateSearchQuery ToQuery() => new()
    {
        Keyword            = Keyword,
        Location           = Location,
        MinExperienceYears = MinExperienceYears,
        MaxExpectedSalary  = MaxExpectedSalary,
        Page               = Page,
        PageSize           = PageSize
    };
}
