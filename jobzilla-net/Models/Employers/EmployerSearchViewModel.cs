using jobzilla_net.Application.Common;
using jobzilla_net.Application.Employers.Dtos;
using jobzilla_net.Application.Employers.Queries;

namespace jobzilla_net.Models.Employers;

/// <summary>
/// View model for the public employer listing page (Employers/Index).
/// Binds query-string filters from the GET request and carries
/// the paged result back to the Razor view.
/// </summary>
public sealed class EmployerSearchViewModel
{
    // ── Filter inputs (bound from query string) ───────────────────────────────

    /// <summary>Free-text keyword (company name).</summary>
    public string? Keyword { get; set; }

    /// <summary>Location text filter.</summary>
    public string? Location { get; set; }

    /// <summary>Industry filter.</summary>
    public string? Industry { get; set; }

    /// <summary>Company size filter.</summary>
    public string? CompanySize { get; set; }

    /// <summary>Current page number. Defaults to 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Items per page. Defaults to 10.</summary>
    public int PageSize { get; set; } = 10;

    // ── Result data (populated by controller) ─────────────────────────────────

    /// <summary>Paged employer results returned from the service.</summary>
    public PagedResult<EmployerSummaryDto> Result { get; set; } =
        PagedResult<EmployerSummaryDto>.Empty(1, 10);

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>True when any filter is actively applied.</summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Keyword) ||
        !string.IsNullOrWhiteSpace(Location) ||
        !string.IsNullOrWhiteSpace(Industry) ||
        !string.IsNullOrWhiteSpace(CompanySize);

    /// <summary>Converts this view model into an Application-layer query.</summary>
    public EmployerSearchQuery ToQuery() => new()
    {
        Keyword     = Keyword,
        Location    = Location,
        Industry    = Industry,
        CompanySize = CompanySize,
        Page        = Page,
        PageSize    = PageSize
    };
}
