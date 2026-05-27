using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;
using jobzilla_net.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class ResumeDataSyncService : IResumeDataSyncService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ResumeDataSyncService> _logger;

    public ResumeDataSyncService(IApplicationDbContext context, ILogger<ResumeDataSyncService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SyncParsedDataAsync(string userId, ParsedResumeDto parsedData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing parsed resume data for user {UserId}", userId);

        var profile = await _context.CandidateProfiles
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Certifications)
            .Include(p => p.SocialLinks)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null)
        {
            _logger.LogWarning("Candidate profile not found for user {UserId}", userId);
            return false;
        }

        // Basic mapping logic (Update if null/empty, or add to collections)
        if (string.IsNullOrWhiteSpace(profile.FullName) && !string.IsNullOrWhiteSpace(parsedData.FullName))
            profile.FullName = parsedData.FullName;

        if (string.IsNullOrWhiteSpace(profile.PhoneNumber) && !string.IsNullOrWhiteSpace(parsedData.PhoneNumber))
            profile.PhoneNumber = parsedData.PhoneNumber;

        foreach (var exp in parsedData.Experiences)
        {
            // Simple deduplication check based on CompanyName and Title
            if (!profile.Experiences.Any(e => e.CompanyName == exp.CompanyName && e.JobTitle == exp.JobTitle))
            {
                profile.Experiences.Add(new CandidateExperience
                {
                    CompanyName = exp.CompanyName ?? "Unknown",
                    JobTitle = exp.JobTitle ?? "Unknown",
                    StartDate = exp.StartDate ?? DateTime.UtcNow,
                    EndDate = exp.EndDate,
                    Description = exp.Description
                });
            }
        }

        foreach (var edu in parsedData.Educations)
        {
            if (!profile.Educations.Any(e => e.InstitutionName == edu.InstitutionName && e.Degree == edu.Degree))
            {
                profile.Educations.Add(new CandidateEducation
                {
                    InstitutionName = edu.InstitutionName ?? "Unknown",
                    Degree = edu.Degree ?? "Unknown",
                    FieldOfStudy = edu.FieldOfStudy ?? "Unknown",
                    StartDate = edu.StartDate ?? DateTime.UtcNow,
                    EndDate = edu.EndDate
                });
            }
        }

        foreach (var cert in parsedData.Certifications)
        {
            if (!profile.Certifications.Any(c => c.Name == cert.Name))
            {
                profile.Certifications.Add(new CandidateCertification
                {
                    Name = cert.Name ?? "Unknown",
                    IssuingOrganization = cert.IssuingOrganization ?? "Unknown",
                    IssueDate = cert.IssueDate ?? DateTime.UtcNow
                });
            }
        }

        foreach (var link in parsedData.SocialLinks)
        {
            if (!profile.SocialLinks.Any(l => l.Url == link.Url))
            {
                profile.SocialLinks.Add(new CandidateSocialLink
                {
                    PlatformName = link.PlatformName ?? "Unknown",
                    Url = link.Url ?? string.Empty
                });
            }
        }

        // Skills logic (requires finding existing skills or creating new ones, then linking)
        // For simplicity in this implementation, we assume skill linking is handled separately or we log it.
        _logger.LogInformation("Parsed {Count} skills. Linking should be done via Skill lookup.", parsedData.Skills.Count);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save synced resume data for user {UserId}", userId);
            return false;
        }
    }
}
