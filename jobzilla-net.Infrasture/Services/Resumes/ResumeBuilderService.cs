using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Resumes.Interfaces;
using jobzilla_net.Application.Resumes.ViewModels;
using jobzilla_net.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class ResumeBuilderService : IResumeBuilderService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ResumeBuilderService> _logger;

    public ResumeBuilderService(IApplicationDbContext context, ILogger<ResumeBuilderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ResumeExportViewModel> GetResumeDataAsync(string userId, CancellationToken cancellationToken = default)
    {
        var profile = await _context.CandidateProfiles
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Certifications)
            .Include(p => p.SocialLinks)
            .Include(p => p.Projects)
            .Include(p => p.Skills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        var vm = new ResumeExportViewModel();

        if (profile != null)
        {
            vm.Profile = new CandidateProfileDto
            {
                FullName = profile.FullName,
                ProfessionalTitle = profile.ProfessionalTitle,
                PhoneNumber = profile.PhoneNumber,
                Location = profile.Location,
                Summary = profile.Summary,
                ExperienceYears = profile.ExperienceYears,
                ExpectedSalary = profile.ExpectedSalary
            };

            vm.Experiences = profile.Experiences.Select(e => new CandidateExperienceViewModel
            {
                Id = e.Id,
                CompanyName = e.CompanyName,
                JobTitle = e.JobTitle,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrentPosition = e.IsCurrentPosition,
                Description = e.Description,
                Location = e.Location
            }).ToList();

            vm.Educations = profile.Educations.Select(e => new CandidateEducationViewModel
            {
                Id = e.Id,
                InstitutionName = e.InstitutionName,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrentlyStudying = e.IsCurrentlyStudying,
                Description = e.Description
            }).ToList();

            vm.Certifications = profile.Certifications.Select(c => new CandidateCertificationViewModel
            {
                Id = c.Id,
                Name = c.Name,
                IssuingOrganization = c.IssuingOrganization,
                IssueDate = c.IssueDate,
                ExpirationDate = c.ExpirationDate,
                CredentialId = c.CredentialId,
                CredentialUrl = c.CredentialUrl
            }).ToList();

            vm.SocialLinks = profile.SocialLinks.Select(s => new CandidateSocialLinkViewModel
            {
                Id = s.Id,
                PlatformName = s.PlatformName,
                Url = s.Url
            }).ToList();

            vm.Projects = profile.Projects.Select(p => new CandidateProjectViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ProjectUrl = p.ProjectUrl,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsOngoing = p.IsOngoing
            }).ToList();

            vm.Skills = profile.Skills
                .Where(cs => cs.Skill != null)
                .Select(cs => cs.Skill!.Name)
                .ToList();
        }

        return vm;
    }

    public async Task<bool> UpdateResumeDataAsync(string userId, ResumeExportViewModel model, CancellationToken cancellationToken = default)
    {
        var profile = await _context.CandidateProfiles
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Certifications)
            .Include(p => p.SocialLinks)
            .Include(p => p.Projects)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null) return false;

        // Update Profile
        if (model.Profile != null)
        {
            profile.FullName = model.Profile.FullName ?? profile.FullName;
            profile.ProfessionalTitle = model.Profile.ProfessionalTitle;
            profile.PhoneNumber = model.Profile.PhoneNumber;
            profile.Location = model.Profile.Location;
            profile.Summary = model.Profile.Summary;
            profile.ExperienceYears = model.Profile.ExperienceYears;
            profile.ExpectedSalary = model.Profile.ExpectedSalary;
        }

        // We replace collections entirely for simplicity of manual edits from UI 
        // (A more advanced approach maps added/updated/deleted based on IDs, but this is simple for forms)
        // Wait, replacing entirely deletes old IDs. Instead, let's map:

        // 1. Experiences
        UpdateCollection(profile.Experiences, model.Experiences, 
            e => e.Id, vm => vm.Id,
            (e, vm) => {
                e.CompanyName = vm.CompanyName; e.JobTitle = vm.JobTitle; e.StartDate = vm.StartDate; 
                e.EndDate = vm.EndDate; e.IsCurrentPosition = vm.IsCurrentPosition; 
                e.Description = vm.Description; e.Location = vm.Location; e.CandidateProfileId = profile.Id;
            },
            vm => new CandidateExperience {
                CompanyName = vm.CompanyName, JobTitle = vm.JobTitle, StartDate = vm.StartDate, 
                EndDate = vm.EndDate, IsCurrentPosition = vm.IsCurrentPosition, 
                Description = vm.Description, Location = vm.Location, CandidateProfileId = profile.Id
            });

        // 2. Educations
        UpdateCollection(profile.Educations, model.Educations, 
            e => e.Id, vm => vm.Id,
            (e, vm) => {
                e.InstitutionName = vm.InstitutionName; e.Degree = vm.Degree; e.FieldOfStudy = vm.FieldOfStudy; 
                e.StartDate = vm.StartDate; e.EndDate = vm.EndDate; e.IsCurrentlyStudying = vm.IsCurrentlyStudying; 
                e.Description = vm.Description; e.CandidateProfileId = profile.Id;
            },
            vm => new CandidateEducation {
                InstitutionName = vm.InstitutionName, Degree = vm.Degree, FieldOfStudy = vm.FieldOfStudy, 
                StartDate = vm.StartDate, EndDate = vm.EndDate, IsCurrentlyStudying = vm.IsCurrentlyStudying, 
                Description = vm.Description, CandidateProfileId = profile.Id
            });

        // 3. Certifications
        UpdateCollection(profile.Certifications, model.Certifications, 
            c => c.Id, vm => vm.Id,
            (c, vm) => {
                c.Name = vm.Name; c.IssuingOrganization = vm.IssuingOrganization; c.IssueDate = vm.IssueDate; 
                c.ExpirationDate = vm.ExpirationDate; c.CredentialId = vm.CredentialId; c.CredentialUrl = vm.CredentialUrl; c.CandidateProfileId = profile.Id;
            },
            vm => new CandidateCertification {
                Name = vm.Name, IssuingOrganization = vm.IssuingOrganization, IssueDate = vm.IssueDate, 
                ExpirationDate = vm.ExpirationDate, CredentialId = vm.CredentialId, CredentialUrl = vm.CredentialUrl, CandidateProfileId = profile.Id
            });

        // 4. Social Links
        UpdateCollection(profile.SocialLinks, model.SocialLinks, 
            s => s.Id, vm => vm.Id,
            (s, vm) => {
                s.PlatformName = vm.PlatformName; s.Url = vm.Url; s.CandidateProfileId = profile.Id;
            },
            vm => new CandidateSocialLink {
                PlatformName = vm.PlatformName, Url = vm.Url, CandidateProfileId = profile.Id
            });

        // 5. Projects
        UpdateCollection(profile.Projects, model.Projects, 
            p => p.Id, vm => vm.Id,
            (p, vm) => {
                p.Name = vm.Name; p.Description = vm.Description; p.ProjectUrl = vm.ProjectUrl; 
                p.StartDate = vm.StartDate; p.EndDate = vm.EndDate; p.IsOngoing = vm.IsOngoing; p.CandidateProfileId = profile.Id;
            },
            vm => new CandidateProject {
                Name = vm.Name, Description = vm.Description, ProjectUrl = vm.ProjectUrl, 
                StartDate = vm.StartDate, EndDate = vm.EndDate, IsOngoing = vm.IsOngoing, CandidateProfileId = profile.Id
            });

        // 6. Skills — sync edited List<string> back to CandidateSkill join table
        if (model.Skills != null)
        {
            // Load current skills with navigation so we can compare names
            var existingSkillLinks = await _context.CandidateSkills
                .Include(cs => cs.Skill)
                .Where(cs => cs.CandidateProfileId == profile.Id)
                .ToListAsync(cancellationToken);

            var submittedNames = model.Skills
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Remove links for skills no longer in the submitted list
            var toRemove = existingSkillLinks
                .Where(cs => cs.Skill == null || !submittedNames.Any(n => n.Equals(cs.Skill.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            foreach (var link in toRemove)
                _context.CandidateSkills.Remove(link);

            // Add links for new skill names
            var existingNames = existingSkillLinks
                .Where(cs => cs.Skill != null)
                .Select(cs => cs.Skill!.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var skillName in submittedNames)
            {
                if (existingNames.Contains(skillName)) continue;

                var skill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower(), cancellationToken)
                    ?? new Skill { Name = skillName };

                if (skill.Id == 0)
                    _context.Skills.Add(skill);

                _context.CandidateSkills.Add(new CandidateSkill
                {
                    CandidateProfileId = profile.Id,
                    Skill              = skill
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void UpdateCollection<TEntity, TVm>(
        ICollection<TEntity> dbCollection, 
        IEnumerable<TVm>? vmCollection, 
        Func<TEntity, int> entityKey, 
        Func<TVm, int> vmKey, 
        Action<TEntity, TVm> updateAction, 
        Func<TVm, TEntity> createAction)
    {
        vmCollection ??= new List<TVm>();

        // Remove deleted items
        var vmKeys = vmCollection.Select(vmKey).Where(k => k != 0).ToList();
        var toRemove = dbCollection.Where(e => !vmKeys.Contains(entityKey(e))).ToList();
        foreach (var item in toRemove) dbCollection.Remove(item);

        // Add or Update
        foreach (var vm in vmCollection)
        {
            if (vmKey(vm) == 0) // New item
            {
                dbCollection.Add(createAction(vm));
            }
            else // Existing item
            {
                var existing = dbCollection.FirstOrDefault(e => entityKey(e) == vmKey(vm));
                if (existing != null)
                {
                    updateAction(existing, vm);
                }
            }
        }
    }

    public async Task<List<jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ResumeTemplates
            .Where(t => t.IsActive)
            .Select(t => new jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                TemplateFilePath = t.TemplateFilePath,
                PreviewImagePath = t.PreviewImagePath
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto?> GetTemplateByIdAsync(int templateId, CancellationToken cancellationToken = default)
    {
        var template = await _context.ResumeTemplates.FindAsync(new object[] { templateId }, cancellationToken);
        if (template == null || !template.IsActive) return null;

        return new jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            TemplateFilePath = template.TemplateFilePath,
            PreviewImagePath = template.PreviewImagePath
        };
    }
}
