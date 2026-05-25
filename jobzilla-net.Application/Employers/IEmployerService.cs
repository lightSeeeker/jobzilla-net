using jobzilla_net.Application.Common;
using jobzilla_net.Application.Employers.Dtos;
using jobzilla_net.Application.Employers.Queries;

namespace jobzilla_net.Application.Employers;

/// <summary>
/// Application-layer contract for all employer-related read operations.
/// Implementations live in the Infrastructure layer.
/// </summary>
public interface IEmployerService
{
    /// <summary>
    /// Returns a paginated, filtered list of active employer profiles.
    /// All parameters in <paramref name="query"/> are optional.
    /// </summary>
    Task<PagedResult<EmployerSummaryDto>> SearchAsync(
        EmployerSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single employer profile by its Id.
    /// Returns <c>null</c> if not found or soft-deleted.
    /// </summary>
    Task<EmployerSummaryDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}
