namespace Jobzilla.Application.Jobs;

public interface IJobService
{
    Task<IReadOnlyList<JobSummaryDto>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default);
    Task<JobSummaryDto?> GetSummaryAsync(int id, CancellationToken cancellationToken = default);
}
