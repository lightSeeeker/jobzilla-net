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
        var pInfo = parsedData.PersonalInfo;
        
        if (!string.IsNullOrWhiteSpace(pInfo.FullName))
            profile.FullName = pInfo.FullName;
        else if (!string.IsNullOrWhiteSpace(pInfo.FirstName) || !string.IsNullOrWhiteSpace(pInfo.LastName))
            profile.FullName = $"{pInfo.FirstName} {pInfo.LastName}".Trim();

        if (!string.IsNullOrWhiteSpace(pInfo.Phone))
            profile.PhoneNumber = pInfo.Phone;

        if (!string.IsNullOrWhiteSpace(parsedData.Summary))
            profile.Summary = parsedData.Summary;

        // Add specific links to SocialLinks if not present
        void AddSocialLink(string platform, string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!profile.SocialLinks.Any(l => l.Url == url))
            {
                profile.SocialLinks.Add(new CandidateSocialLink { PlatformName = platform, Url = url, CandidateProfileId = profile.Id });
            }
        }
        AddSocialLink("LinkedIn", pInfo.Linkedin);
        AddSocialLink("GitHub", pInfo.Github);
        AddSocialLink("Portfolio", pInfo.Portfolio);

        // ── Experience ────────────────────────────────────────────────────────
        foreach (var exp in parsedData.Experience)
        {
            if (!profile.Experiences.Any(e => e.CompanyName == exp.Company && e.JobTitle == exp.Title))
            {
                profile.Experiences.Add(new CandidateExperience
                {
                    CompanyName        = exp.Company ?? string.Empty,
                    JobTitle           = exp.Title   ?? string.Empty,
                    Location           = exp.Location ?? string.Empty,
                    StartDate          = ParseDateString(exp.StartDate) ?? DateTime.UtcNow,
                    EndDate            = exp.IsCurrent ? null : ParseDateString(exp.EndDate),
                    Description        = exp.Bullets.Count > 0 ? string.Join(Environment.NewLine, exp.Bullets.Select(b => "• " + b)) : string.Empty,
                    CandidateProfileId = profile.Id
                });
            }
        }

        // ── Education ─────────────────────────────────────────────────────────
        foreach (var edu in parsedData.Education)
        {
            if (!profile.Educations.Any(e => e.InstitutionName == edu.Institution && e.Degree == edu.Degree))
            {
                profile.Educations.Add(new CandidateEducation
                {
                    InstitutionName    = edu.Institution ?? string.Empty,
                    Degree             = edu.Degree      ?? string.Empty,
                    FieldOfStudy       = edu.Field         ?? string.Empty,
                    StartDate          = edu.StartYear.HasValue ? new DateTime(edu.StartYear.Value, 1, 1) : DateTime.UtcNow,
                    EndDate            = edu.EndYear.HasValue ? new DateTime(edu.EndYear.Value, 1, 1) : null,
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
                    IssueDate           = ParseDateString(cert.IssueDate) ?? DateTime.UtcNow,
                    CandidateProfileId  = profile.Id
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
                    StartDate           = DateTime.UtcNow, // Custom rules didn't explicitly demand project dates, but we could add if needed
                    Description         = proj.Highlights.Count > 0 ? string.Join(Environment.NewLine, proj.Highlights.Select(b => "• " + b)) : string.Empty,
                    CandidateProfileId  = profile.Id
                });
            }
        }

        // ── Skills ────────────────────────────────────────────────────────────
        var existingSkillNames = profile.Skills
            .Where(cs => cs.Skill != null)
            .Select(cs => cs.Skill!.Name.ToLowerInvariant())
            .ToHashSet();

        // Flatten the categorized skills
        var allSkills = parsedData.Skills.SelectMany(kv => kv.Value).Distinct(StringComparer.OrdinalIgnoreCase);

        int skillsAdded = 0;
        foreach (var skillName in allSkills)
        {
            if (existingSkillNames.Contains(skillName.ToLowerInvariant()))
                continue;

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
            skillsAdded++;
        }

        _logger.LogInformation("Synced {Count} skills for user {UserId}", skillsAdded, userId);

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

    private static DateTime? ParseDateString(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        var parts = dateStr.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int month))
        {
            return new DateTime(year, month, 1);
        }
        if (parts.Length == 1 && int.TryParse(parts[0], out int y))
        {
            return new DateTime(y, 1, 1);
        }
        return null;
    }
}

