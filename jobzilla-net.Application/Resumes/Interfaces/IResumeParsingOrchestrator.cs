namespace jobzilla_net.Application.Resumes.Interfaces;

/// <summary>
/// Orchestrates the end-to-end parsing flow: Extraction -> Parsing -> Sync.
/// </summary>
public interface IResumeParsingOrchestrator
{
    Task<bool> ParseAndSyncResumeAsync(string userId, string filePath, CancellationToken cancellationToken = default);
}
