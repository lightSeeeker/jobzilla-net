using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class ResumeParsingOrchestrator : IResumeParsingOrchestrator
{
    private readonly IResumeFileExtractor _extractor;
    private readonly IResumeTextParser _parser;
    private readonly IResumeDataSyncService _syncService;
    private readonly ILogger<ResumeParsingOrchestrator> _logger;

    public ResumeParsingOrchestrator(
        IResumeFileExtractor extractor,
        IResumeTextParser parser,
        IResumeDataSyncService syncService,
        ILogger<ResumeParsingOrchestrator> logger)
    {
        _extractor = extractor;
        _parser = parser;
        _syncService = syncService;
        _logger = logger;
    }

    public async Task<bool> ParseAndSyncResumeAsync(string userId, string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting resume parsing orchestration for user {UserId} with file {FilePath}", userId, filePath);

        try
        {
            // 1. Extract raw text from file
            var rawText = await _extractor.ExtractTextAsync(filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(rawText))
            {
                _logger.LogWarning("No text could be extracted from {FilePath}", filePath);
                return false;
            }

            // 2. Parse text into structured DTO
            var parsedDto = await _parser.ParseAsync(rawText, cancellationToken);

            // 3. Sync structured DTO to CandidateProfile database entities
            var syncResult = await _syncService.SyncParsedDataAsync(userId, parsedDto, cancellationToken);

            return syncResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parsing orchestration failed for user {UserId} with file {FilePath}", userId, filePath);
            return false; // Handle parsing failures gracefully
        }
    }
}
