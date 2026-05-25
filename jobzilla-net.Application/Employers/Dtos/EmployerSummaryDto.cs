namespace jobzilla_net.Application.Employers.Dtos;

/// <summary>
/// Projection DTO for employer listing cards and search results.
/// Populated exclusively by EF projection — entity is never exposed directly.
/// </summary>
public sealed class EmployerSummaryDto
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? Location { get; init; }
    public string? LogoPath { get; init; }
    public bool IsVerified { get; init; }
    
    /// <summary>Total number of active (published) jobs for this employer.</summary>
    public int ActiveJobCount { get; init; }

    // Metadata
    public DateTime CreatedAtUtc { get; init; }

    // Full detail fields — populated only by GetByIdAsync
    public string? Description { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? BannerPath { get; init; }
}
