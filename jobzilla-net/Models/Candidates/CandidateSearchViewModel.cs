using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Candidates.Queries;
using jobzilla_net.Application.Common;
using jobzilla_net.Core.Entities;

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

    /// <summary>Category filter. Matches jobs the candidate has applied to or saved. Null = no filter.</summary>
    public int? CategoryId { get; set; }

    /// <summary>Skill filter. Matches candidate's associated skills. Null = no filter.</summary>
    public int? SkillId { get; set; }

    /// <summary>Current page number. Defaults to 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Items per page. Defaults to 10.</summary>
    public int PageSize { get; set; } = 10;

    // ── Result data (populated by controller) ─────────────────────────────────

    /// <summary>Paged candidate results returned from the service.</summary>
    public PagedResult<CandidateSummaryDto> Result { get; set; } =
        PagedResult<CandidateSummaryDto>.Empty(1, 10);

    /// <summary>
    /// All active categories for the filter sidebar.
    /// Populated by the controller from the database.
    /// </summary>
    public IReadOnlyList<JobCategory> Categories { get; set; } = Array.Empty<JobCategory>();

    /// <summary>
    /// All active skills for the filter sidebar.
    /// Populated by the controller from the database.
    /// </summary>
    public IReadOnlyList<Skill> Skills { get; set; } = Array.Empty<Skill>();

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>True when any filter is actively applied.</summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Keyword) ||
        !string.IsNullOrWhiteSpace(Location) ||
        MinExperienceYears.HasValue ||
        MaxExpectedSalary.HasValue ||
        CategoryId.HasValue ||
        SkillId.HasValue;

    /// <summary>Converts this view model into an Application-layer query.</summary>
    public CandidateSearchQuery ToQuery() => new()
    {
        Keyword            = Keyword,
        Location           = Location,
        MinExperienceYears = MinExperienceYears,
        MaxExpectedSalary  = MaxExpectedSalary,
        CategoryId         = CategoryId,
        SkillId            = SkillId,
        Page               = Page,
        PageSize           = PageSize
    };
}
