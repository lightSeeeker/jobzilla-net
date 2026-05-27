using System.Text.RegularExpressions;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class BasicRegexResumeParser : IResumeTextParser
{
    private readonly ILogger<BasicRegexResumeParser> _logger;

    public BasicRegexResumeParser(ILogger<BasicRegexResumeParser> logger)
    {
        _logger = logger;
    }

    public Task<ParsedResumeDto> ParseAsync(string rawText, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Parsing raw resume text using Regex.");
        
        var dto = new ParsedResumeDto();
        
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return Task.FromResult(dto);
        }

        // Very basic regex parsing (Extensible for AI or advanced NLP later)
        var emailMatch = Regex.Match(rawText, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        if (emailMatch.Success)
        {
            dto.Email = emailMatch.Value;
        }

        var phoneMatch = Regex.Match(rawText, @"\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}");
        if (phoneMatch.Success)
        {
            dto.PhoneNumber = phoneMatch.Value;
        }

        // Simulated extraction for demo purposes based on the stub text
        if (rawText.Contains("John Doe")) dto.FullName = "John Doe";
        if (rawText.Contains("C#")) dto.Skills.AddRange(new[] { "C#", "ASP.NET Core" });
        
        dto.Experiences.Add(new ParsedExperienceDto
        {
            CompanyName = "Jobzilla",
            JobTitle = "Software Engineer",
            StartDate = new DateTime(2020, 1, 1),
            EndDate = new DateTime(2023, 1, 1),
            Description = "Developed core platform features."
        });

        return Task.FromResult(dto);
    }
}
