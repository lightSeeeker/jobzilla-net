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

        // Load all required navigation properties in a single query
        var profile = await _context.CandidateProfiles
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Certifications)
            .Include(p => p.SocialLinks)
            .Include(p => p.Projects)
            .Include(p => p.Skills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null)
        {
            _logger.LogWarning("Candidate profile not found for user {UserId}", userId);
            return false;
        }

        // ── Personal Information ──────────────────────────────────────────────
        // Parsed data is PRIMARY. Profile fields are fallback when parser extracted nothing.
        if (!string.IsNullOrWhiteSpace(parsedData.FullName))
            profile.FullName = parsedData.FullName;

        if (!string.IsNullOrWhiteSpace(parsedData.PhoneNumber))
            profile.PhoneNumber = parsedData.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(parsedData.Summary))
            profile.Summary = parsedData.Summary;

        // ── Experience ────────────────────────────────────────────────────────
        foreach (var exp in parsedData.Experiences)
        {
            if (!profile.Experiences.Any(e => e.CompanyName == exp.CompanyName && e.JobTitle == exp.JobTitle))
            {
                profile.Experiences.Add(new CandidateExperience
                {
                    CompanyName        = exp.CompanyName ?? string.Empty,
                    JobTitle           = exp.JobTitle    ?? string.Empty,
                    StartDate          = exp.StartDate   ?? DateTime.UtcNow,
                    EndDate            = exp.EndDate,
                    Description        = exp.Description,
                    CandidateProfileId = profile.Id
                });
            }
        }

        // ── Education ─────────────────────────────────────────────────────────
        foreach (var edu in parsedData.Educations)
        {
            if (!profile.Educations.Any(e => e.InstitutionName == edu.InstitutionName && e.Degree == edu.Degree))
            {
                profile.Educations.Add(new CandidateEducation
                {
                    InstitutionName    = edu.InstitutionName ?? string.Empty,
                    Degree             = edu.Degree          ?? string.Empty,
                    FieldOfStudy       = edu.FieldOfStudy    ?? string.Empty,
                    StartDate          = edu.StartDate       ?? DateTime.UtcNow,
                    EndDate            = edu.EndDate,
                    CandidateProfileId = profile.Id
                });
            }
        }

        // ── Certifications ────────────────────────────────────────────────────
        foreach (var cert in parsedData.Certifications)
        {
            if (!profile.Certifications.Any(c => c.Name == cert.Name))
            {
                profile.Certifications.Add(new CandidateCertification
                {
                    Name                = cert.Name                ?? string.Empty,
                    IssuingOrganization = cert.IssuingOrganization ?? string.Empty,
                    IssueDate           = cert.IssueDate           ?? DateTime.UtcNow,
                    CandidateProfileId  = profile.Id
                });
            }
        }

        // ── Social Links ──────────────────────────────────────────────────────
        foreach (var link in parsedData.SocialLinks)
        {
            if (!profile.SocialLinks.Any(l => l.Url == link.Url))
            {
                profile.SocialLinks.Add(new CandidateSocialLink
                {
                    PlatformName       = link.PlatformName ?? string.Empty,
                    Url                = link.Url          ?? string.Empty,
                    CandidateProfileId = profile.Id
                });
            }
        }

        // ── Projects ──────────────────────────────────────────────────────────
        foreach (var proj in parsedData.Projects)
        {
            if (!profile.Projects.Any(p => p.Name == proj.Name))
            {
                profile.Projects.Add(new CandidateProject
                {
                    Name                = proj.Name ?? string.Empty,
                    StartDate           = proj.StartDate ?? DateTime.UtcNow,
                    EndDate             = proj.EndDate,
                    ProjectUrl          = proj.ProjectUrl,
                    Description         = proj.Description,
                    CandidateProfileId  = profile.Id
                });
            }
        }

        // ── Skills ────────────────────────────────────────────────────────────
        // Skills use a normalised lookup table (Skill) + join table (CandidateSkill).
        // For each parsed skill name, upsert the Skill row then link it to the profile.
        var existingSkillNames = profile.Skills
            .Where(cs => cs.Skill != null)
            .Select(cs => cs.Skill!.Name.ToLowerInvariant())
            .ToHashSet();

        foreach (var skillName in parsedData.Skills.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (existingSkillNames.Contains(skillName.ToLowerInvariant()))
                continue;

            // Find or create the global Skill record
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower(), cancellationToken)
                ?? new Skill { Name = skillName };

            if (skill.Id == 0)
                _context.Skills.Add(skill);

            profile.Skills.Add(new CandidateSkill
            {
                Skill              = skill,
                CandidateProfileId = profile.Id
            });

            existingSkillNames.Add(skillName.ToLowerInvariant());
        }

        _logger.LogInformation("Synced {Count} skills for user {UserId}", parsedData.Skills.Count, userId);

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
