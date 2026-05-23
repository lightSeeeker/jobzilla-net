namespace jobzilla_net.Application.Jobs.Dtos;

/// <summary>
/// Projection DTO for job listing cards and search results.
/// Populated exclusively by EF projection — entity is never exposed directly.
/// </summary>
public sealed class JobSummaryDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Slug { get; init; }

    // Employer
    public string CompanyName { get; init; } = string.Empty;
    public string? CompanyLogoPath { get; init; }

    // Categorisation
    public string CategoryName { get; init; } = string.Empty;
    public string? CategoryIconCss { get; init; }

    // Position details
    public string Location { get; init; } = string.Empty;
    public string EmploymentType { get; init; } = string.Empty;
    public decimal? MinimumSalary { get; init; }
    public decimal? MaximumSalary { get; init; }

    // Metadata
    public bool IsFeatured { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }

    // Full detail fields — populated only by GetByIdAsync
    public string? Description { get; init; }
    public string? Requirements { get; init; }
    public string? Responsibilities { get; init; }
}
