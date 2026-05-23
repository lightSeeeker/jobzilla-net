namespace jobzilla_net.Application.Candidates.Dtos;

/// <summary>
/// Projection DTO for candidate listing cards and search results.
/// Populated exclusively by EF projection — entity is never exposed directly.
/// </summary>
public sealed class CandidateSummaryDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? ProfessionalTitle { get; init; }
    public string? Location { get; init; }
    public string? ProfileImagePath { get; init; }

    // Experience
    public int? ExperienceYears { get; init; }
    public decimal? ExpectedSalary { get; init; }

    // Metadata
    public DateTime CreatedAtUtc { get; init; }

    // Full detail fields — populated only by GetByIdAsync
    public string? Summary { get; init; }
}
