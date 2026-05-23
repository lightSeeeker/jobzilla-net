using jobzilla_net.Application.Common;
using jobzilla_net.Application.Jobs.Dtos;
using jobzilla_net.Application.Jobs.Queries;
using jobzilla_net.Core.Entities;

namespace jobzilla_net.Models.Jobs;

/// <summary>
/// View model for the public job listing page (Jobs/Index).
/// Binds query-string filters from the GET request and carries
/// the paged result + category list back to the Razor view.
/// </summary>
public sealed class JobSearchViewModel
{
    // ── Filter inputs (bound from query string) ───────────────────────────────

    /// <summary>Free-text keyword (title, description, company).</summary>
    public string? Keyword { get; set; }

    /// <summary>Location text filter.</summary>
    public string? Location { get; set; }

    /// <summary>Optional category filter. Null = all categories.</summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Employment type filter. Matches <see cref="jobzilla_net.Core.Enums.EmploymentType"/> names.
    /// Null = all types.
    /// </summary>
    public string? EmploymentType { get; set; }

    /// <summary>Minimum salary filter (inclusive). Null = no filter.</summary>
    public decimal? MinSalary { get; set; }

    /// <summary>Maximum salary filter (inclusive). Null = no filter.</summary>
    public decimal? MaxSalary { get; set; }

    /// <summary>Current page number. Defaults to 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Items per page. Defaults to 10.</summary>
    public int PageSize { get; set; } = 10;

    // ── Result data (populated by controller) ─────────────────────────────────

    /// <summary>Paged job results returned from the service.</summary>
    public PagedResult<JobSummaryDto> Result { get; set; } = PagedResult<JobSummaryDto>.Empty(1, 10);

    /// <summary>
    /// All active categories for the filter sidebar.
    /// Populated by the controller from the database.
    /// </summary>
    public IReadOnlyList<JobCategory> Categories { get; set; } = Array.Empty<JobCategory>();

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>True when any filter is actively applied.</summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Keyword) ||
        !string.IsNullOrWhiteSpace(Location) ||
        CategoryId.HasValue ||
        !string.IsNullOrWhiteSpace(EmploymentType) ||
        MinSalary.HasValue ||
        MaxSalary.HasValue;

    /// <summary>Converts this view model into an Application-layer query.</summary>
    public JobSearchQuery ToQuery() => new()
    {
        Keyword        = Keyword,
        Location       = Location,
        CategoryId     = CategoryId,
        EmploymentType = EmploymentType,
        MinSalary      = MinSalary,
        MaxSalary      = MaxSalary,
        Page           = Page,
        PageSize       = PageSize
    };
}
