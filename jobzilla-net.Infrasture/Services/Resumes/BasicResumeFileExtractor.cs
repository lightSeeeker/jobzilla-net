using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class BasicResumeFileExtractor : IResumeFileExtractor
{
    private readonly ILogger<BasicResumeFileExtractor> _logger;

    public BasicResumeFileExtractor(ILogger<BasicResumeFileExtractor> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting text from {FilePath}. Note: A recommended library (e.g. iText7 or OpenXML) should be used in production.", filePath);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Resume file not found.", filePath);
        }

        // Simplistic stub for extraction. In a real scenario, we'd use a dedicated library.
        // For now, we simulate extraction returning some dummy text if it's a binary file,
        // or read it directly if it happens to be plain text.
        
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension == ".txt")
        {
            return await File.ReadAllTextAsync(filePath, cancellationToken);
        }

        return "STUB_EXTRACTED_TEXT: Name: John Doe\nEmail: john@example.com\nPhone: 123-456-7890\nSkills: C#, ASP.NET Core\nExperience: Software Engineer at Jobzilla (2020-2023)";
    }
}
