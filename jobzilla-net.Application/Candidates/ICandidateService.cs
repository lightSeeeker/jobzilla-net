using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Candidates.Queries;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Application.Candidates;

/// <summary>
/// Application-layer contract for all candidate-related read operations.
/// Implementations live in the Infrastructure layer.
/// </summary>
public interface ICandidateService
{
    /// <summary>
    /// Returns a paginated, filtered list of active candidate profiles.
    /// All parameters in <paramref name="query"/> are optional.
    /// </summary>
    Task<PagedResult<CandidateSummaryDto>> SearchAsync(
        CandidateSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single candidate profile by its Id.
    /// Returns <c>null</c> if not found or soft-deleted.
    /// </summary>
    Task<CandidateSummaryDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}
