using jobzilla_net.Application.Resumes.Dtos;

namespace jobzilla_net.Application.Resumes.Interfaces;

/// <summary>
/// Responsible for mapping and saving parsed resume data into the Candidate Profile entities.
/// </summary>
public interface IResumeDataSyncService
{
    Task<bool> SyncParsedDataAsync(string userId, ParsedResumeDto parsedData, CancellationToken cancellationToken = default);
}
