namespace jobzilla_net.Application.Resumes.Interfaces;

/// <summary>
/// Responsible for extracting raw text from uploaded document files (PDF/DOCX).
/// </summary>
public interface IResumeFileExtractor
{
    Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default);
}
