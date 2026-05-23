using jobzilla_net.Application.Common;
using jobzilla_net.Application.Jobs.Dtos;
using jobzilla_net.Application.Jobs.Queries;

namespace jobzilla_net.Application.Jobs;

/// <summary>
/// Application-layer contract for all job-related read operations.
/// Implementations live in the Infrastructure layer.
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Returns a paginated, filtered list of published jobs.
    /// All parameters in <paramref name="query"/> are optional.
    /// </summary>
    Task<PagedResult<JobSummaryDto>> SearchAsync(
        JobSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single published job by its Id.
    /// Returns <c>null</c> if not found or not published.
    /// </summary>
    Task<JobSummaryDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}
