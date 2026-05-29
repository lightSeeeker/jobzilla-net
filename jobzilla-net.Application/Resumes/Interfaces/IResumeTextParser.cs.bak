using jobzilla_net.Application.Resumes.Dtos;

namespace jobzilla_net.Application.Resumes.Interfaces;

/// <summary>
/// Responsible for parsing raw text into structured resume fields.
/// Designed to be extensible for regex-based or AI-based parsing.
/// </summary>
public interface IResumeTextParser
{
    Task<ParsedResumeDto> ParseAsync(string rawText, CancellationToken cancellationToken = default);
}
