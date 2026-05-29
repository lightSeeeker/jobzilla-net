using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Resumes.Dtos;
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

    public async Task<ResumeExportViewModel> GetResumeDataAsync(string userId, int? resumeId = null, CancellationToken cancellationToken = default)
    {
        if (resumeId.HasValue)
        {
            var customResumeInfo = await _context.CandidateResumes
                .Where(r => r.Id == resumeId.Value && r.CandidateProfile!.UserId == userId)
                .Select(r => new { r.Id })
                .FirstOrDefaultAsync(cancellationToken);

            if (customResumeInfo != null)
            {
                var doc = await GetResumeDocumentAsync(userId, resumeId, cancellationToken);
                var customVm = new ResumeExportViewModel
                {
                    Profile = new CandidateProfileDto
                    {
                        FullName = doc.FullName ?? "",
                        ProfessionalTitle = doc.ProfessionalTitle,
                        PhoneNumber = doc.Phone,
                        Location = doc.Location,
                        Summary = doc.Summary
                    }
                };

                if (!string.IsNullOrWhiteSpace(doc.LinkedInUrl)) customVm.SocialLinks.Add(new CandidateSocialLinkViewModel { PlatformName = "LinkedIn", Url = doc.LinkedInUrl });
                if (!string.IsNullOrWhiteSpace(doc.GitHubUrl)) customVm.SocialLinks.Add(new CandidateSocialLinkViewModel { PlatformName = "GitHub", Url = doc.GitHubUrl });
                if (!string.IsNullOrWhiteSpace(doc.Website)) customVm.SocialLinks.Add(new CandidateSocialLinkViewModel { PlatformName = "Website", Url = doc.Website });

                foreach (var sec in doc.Sections)
                {
                    if (sec.SectionType == jobzilla_net.Application.Resumes.Dtos.ResumeBuilderSectionType.Experience)
                    {
                        foreach (var itm in sec.Items)
                            customVm.Experiences.Add(new CandidateExperienceViewModel {
                                JobTitle = itm.GetValue("JobTitle"), CompanyName = itm.GetValue("CompanyName"), Location = itm.GetValue("Location"),
                                StartDate = DateTime.TryParse(itm.GetValue("StartDate"), out var sd) ? sd : default,
                                EndDate = DateTime.TryParse(itm.GetValue("EndDate"), out var ed) ? ed : null,
                                IsCurrentPosition = itm.GetValue("IsCurrent") == "true", Description = itm.GetValue("Description")
                            });
                    }
                    else if (sec.SectionType == jobzilla_net.Application.Resumes.Dtos.ResumeBuilderSectionType.Education)
                    {
                        foreach (var itm in sec.Items)
                            customVm.Educations.Add(new CandidateEducationViewModel {
                                InstitutionName = itm.GetValue("InstitutionName"), Degree = itm.GetValue("Degree"), FieldOfStudy = itm.GetValue("FieldOfStudy"),
                                StartDate = DateTime.TryParse(itm.GetValue("StartDate"), out var sd) ? sd : default,
                                EndDate = DateTime.TryParse(itm.GetValue("EndDate"), out var ed) ? ed : null,
                                IsCurrentlyStudying = itm.GetValue("IsCurrent") == "true", Description = itm.GetValue("Description")
                            });
                    }
                    else if (sec.SectionType == jobzilla_net.Application.Resumes.Dtos.ResumeBuilderSectionType.Skills)
                    {
                        foreach (var itm in sec.Items) if (!string.IsNullOrWhiteSpace(itm.GetValue("Name"))) customVm.Skills.Add(itm.GetValue("Name")!);
                    }
                    else if (sec.SectionType == jobzilla_net.Application.Resumes.Dtos.ResumeBuilderSectionType.Certifications)
                    {
                        foreach (var itm in sec.Items)
                            customVm.Certifications.Add(new CandidateCertificationViewModel {
                                Name = itm.GetValue("Name"), IssuingOrganization = itm.GetValue("IssuingOrganization"),
                                IssueDate = DateTime.TryParse(itm.GetValue("IssueDate"), out var sd) ? sd : default,
                                ExpirationDate = DateTime.TryParse(itm.GetValue("ExpirationDate"), out var ed) ? ed : null,
                                CredentialId = itm.GetValue("CredentialId"), CredentialUrl = itm.GetValue("CredentialUrl")
                            });
                    }
                    else if (sec.SectionType == jobzilla_net.Application.Resumes.Dtos.ResumeBuilderSectionType.Projects)
                    {
                        foreach (var itm in sec.Items)
                            customVm.Projects.Add(new CandidateProjectViewModel {
                                Name = itm.GetValue("Name"), Description = itm.GetValue("Description"), ProjectUrl = itm.GetValue("ProjectUrl"),
                                StartDate = DateTime.TryParse(itm.GetValue("StartDate"), out var sd) ? sd : default,
                                EndDate = DateTime.TryParse(itm.GetValue("EndDate"), out var ed) ? ed : null,
                                IsOngoing = itm.GetValue("IsOngoing") == "true"
                            });
                    }
                }
                return customVm;
            }
        }

        return new ResumeExportViewModel();
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
        var template = await _context.ResumeTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive, cancellationToken);
            
        if (template == null) return null;

        return new jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            TemplateFilePath = template.TemplateFilePath,
            PreviewImagePath = template.PreviewImagePath
        };
    }

    public async Task<bool> SetTemplateForResumeAsync(string userId, int resumeId, int templateId, CancellationToken cancellationToken = default)
    {
        var profileId = await GetProfileIdAsync(userId, cancellationToken);
        if (profileId == null) return false;

        var resume = await _context.CandidateResumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.CandidateProfileId == profileId.Value, cancellationToken);

        if (resume == null) return false;

        resume.TemplateId = templateId;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ── Dynamic Document API ──────────────────────────────────────────────────

    public async Task<ResumeDocument> GetResumeDocumentAsync(string userId, int? resumeId = null, CancellationToken cancellationToken = default)
    {
        if (!resumeId.HasValue) return new ResumeDocument();

        var customResume = await _context.CandidateResumes
            .Where(r => r.Id == resumeId.Value && r.CandidateProfile!.UserId == userId)
            .Select(r => new { r.IsBuilderGenerated, r.DocumentData })
            .FirstOrDefaultAsync(cancellationToken);

        if (customResume == null) return new ResumeDocument();

        // Check for saved JSON resume data first (for both builder-generated and edited uploaded resumes)
        if (!string.IsNullOrWhiteSpace(customResume.DocumentData))
        {
            try
            {
                var docData = System.Text.Json.JsonSerializer.Deserialize<ResumeDocument>(
                    customResume.DocumentData, 
                    new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                if (docData != null) return docData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON PARSE ERROR]: {ex.Message}");
                // fallback if parsing fails
            }
        }

        // If it's a builder-generated resume but missing JSON data, return empty (clean scratch resume)
        if (customResume.IsBuilderGenerated) return new ResumeDocument();

        // FALLBACK: For an uploaded resume that hasn't been edited yet, initialize it from the global profile.
        var profile = await _context.CandidateProfiles
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Certifications)
            .Include(p => p.SocialLinks)
            .Include(p => p.Projects)
            .Include(p => p.References)
            .Include(p => p.Skills).ThenInclude(cs => cs.Skill)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        var doc = new ResumeDocument();

        if (profile == null)
            return doc;

        // Personal info
        doc.FullName           = profile.FullName;
        doc.ProfessionalTitle  = profile.ProfessionalTitle;
        doc.Phone              = profile.PhoneNumber;
        doc.Location           = profile.Location;
        doc.Summary            = profile.Summary;

        // Social links — extract LinkedIn / GitHub separately
        doc.LinkedInUrl = profile.SocialLinks
            .FirstOrDefault(s => s.PlatformName != null && s.PlatformName.Contains("linkedin", StringComparison.OrdinalIgnoreCase))?.Url;
        doc.GitHubUrl = profile.SocialLinks
            .FirstOrDefault(s => s.PlatformName != null && s.PlatformName.Contains("github", StringComparison.OrdinalIgnoreCase))?.Url;
        doc.Website = profile.SocialLinks
            .FirstOrDefault(s => s.PlatformName != null &&
                !s.PlatformName.Contains("linkedin", StringComparison.OrdinalIgnoreCase) &&
                !s.PlatformName.Contains("github", StringComparison.OrdinalIgnoreCase))?.Url;

        int order = 0;

        // ── Experience ──────────────────────────────────────────────────────
        if (profile.Experiences.Any())
        {
            var section = new ResumeSection
            {
                Id           = "exp",
                SectionType  = ResumeBuilderSectionType.Experience,
                Title        = "Work Experience",
                DisplayOrder = order++,
                IsVisible    = true
            };
            int itemOrder = 0;
            foreach (var e in profile.Experiences.OrderByDescending(x => x.StartDate))
            {
                var item = new ResumeSectionItem { Id = $"exp_{e.Id}", DisplayOrder = itemOrder++ };
                item.SetValue("JobTitle",    e.JobTitle,    "Job Title");
                item.SetValue("CompanyName", e.CompanyName, "Company Name");
                item.SetValue("Location",    e.Location,    "Location");
                item.SetValue("StartDate",   e.StartDate == default ? null : e.StartDate.ToString("yyyy-MM-dd"), "Start Date", ResumeFieldType.Date);
                item.SetValue("EndDate",     e.EndDate?.ToString("yyyy-MM-dd"),  "End Date",   ResumeFieldType.Date);
                item.SetValue("IsCurrent",   e.IsCurrentPosition ? "true" : "false", "Current Position", ResumeFieldType.Checkbox);
                item.SetValue("Description", e.Description, "Description", ResumeFieldType.Textarea);
                section.Items.Add(item);
            }
            doc.Sections.Add(section);
        }

        // ── Education ───────────────────────────────────────────────────────
        if (profile.Educations.Any())
        {
            var section = new ResumeSection
            {
                Id           = "edu",
                SectionType  = ResumeBuilderSectionType.Education,
                Title        = "Education",
                DisplayOrder = order++,
                IsVisible    = true
            };
            int itemOrder = 0;
            foreach (var e in profile.Educations.OrderByDescending(x => x.StartDate))
            {
                var item = new ResumeSectionItem { Id = $"edu_{e.Id}", DisplayOrder = itemOrder++ };
                item.SetValue("InstitutionName", e.InstitutionName, "Institution");
                item.SetValue("Degree",          e.Degree,          "Degree");
                item.SetValue("FieldOfStudy",    e.FieldOfStudy,    "Field of Study");
                item.SetValue("StartDate",       e.StartDate == default ? null : e.StartDate.ToString("yyyy-MM-dd"), "Start Date", ResumeFieldType.Date);
                item.SetValue("EndDate",         e.EndDate?.ToString("yyyy-MM-dd"),  "End Date",   ResumeFieldType.Date);
                item.SetValue("IsCurrent",       e.IsCurrentlyStudying ? "true" : "false", "Currently Studying", ResumeFieldType.Checkbox);
                item.SetValue("Description",     e.Description, "Description", ResumeFieldType.Textarea);
                section.Items.Add(item);
            }
            doc.Sections.Add(section);
        }

        // ── Skills ──────────────────────────────────────────────────────────
        var skillNames = profile.Skills.Where(cs => cs.Skill != null).Select(cs => cs.Skill!.Name).ToList();
        if (skillNames.Any())
        {
            var section = new ResumeSection
            {
                Id           = "skills",
                SectionType  = ResumeBuilderSectionType.Skills,
                Title        = "Skills",
                DisplayOrder = order++,
                IsVisible    = true
            };
            int itemOrder = 0;
            foreach (var sk in skillNames)
            {
                var item = new ResumeSectionItem { Id = $"sk_{itemOrder}", DisplayOrder = itemOrder++ };
                item.SetValue("Name", sk, "Skill");
                section.Items.Add(item);
            }
            doc.Sections.Add(section);
        }

        // ── Certifications ──────────────────────────────────────────────────
        if (profile.Certifications.Any())
        {
            var section = new ResumeSection
            {
                Id           = "certs",
                SectionType  = ResumeBuilderSectionType.Certifications,
                Title        = "Certifications",
                DisplayOrder = order++,
                IsVisible    = true
            };
            int itemOrder = 0;
            foreach (var c in profile.Certifications.OrderByDescending(x => x.IssueDate))
            {
                var item = new ResumeSectionItem { Id = $"cert_{c.Id}", DisplayOrder = itemOrder++ };
                item.SetValue("Name",                c.Name,                "Certification Name");
                item.SetValue("IssuingOrganization", c.IssuingOrganization, "Issuing Organization");
                item.SetValue("IssueDate",           c.IssueDate == default ? null : c.IssueDate.ToString("yyyy-MM-dd"), "Issue Date",      ResumeFieldType.Date);
                item.SetValue("ExpirationDate",      c.ExpirationDate?.ToString("yyyy-MM-dd"), "Expiration Date", ResumeFieldType.Date);
                item.SetValue("CredentialId",        c.CredentialId,  "Credential ID");
                item.SetValue("CredentialUrl",       c.CredentialUrl, "Credential URL", ResumeFieldType.Url);
                section.Items.Add(item);
            }
            doc.Sections.Add(section);
        }

        // ── Projects ────────────────────────────────────────────────────────
        if (profile.Projects.Any())
        {
            var section = new ResumeSection
            {
                Id           = "projects",
                SectionType  = ResumeBuilderSectionType.Projects,
                Title        = "Projects",
                DisplayOrder = order++,
                IsVisible    = true
            };
            int itemOrder = 0;
            foreach (var p in profile.Projects.OrderByDescending(x => x.StartDate))
            {
                var item = new ResumeSectionItem { Id = $"proj_{p.Id}", DisplayOrder = itemOrder++ };
                item.SetValue("Name",        p.Name,        "Project Name");
                item.SetValue("ProjectUrl",  p.ProjectUrl,  "Project URL", ResumeFieldType.Url);
                item.SetValue("StartDate",   p.StartDate?.ToString("yyyy-MM-dd"), "Start Date", ResumeFieldType.Date);
                item.SetValue("EndDate",     p.EndDate?.ToString("yyyy-MM-dd"),   "End Date",   ResumeFieldType.Date);
                item.SetValue("IsOngoing",   p.IsOngoing ? "true" : "false", "Ongoing", ResumeFieldType.Checkbox);
                item.SetValue("Description", p.Description, "Description", ResumeFieldType.Textarea);
                section.Items.Add(item);
            }
            doc.Sections.Add(section);
        }

        // ── References ──────────────────────────────────────────────────────
        if (profile.References.Any())
        {
            var section = new ResumeSection
            {
                Id           = "refs",
                SectionType  = ResumeBuilderSectionType.References,
                Title        = "References",
                DisplayOrder = order++,
                IsVisible    = true
            };
            int itemOrder = 0;
            foreach (var r in profile.References.OrderBy(x => x.DisplayOrder))
            {
                var item = new ResumeSectionItem { Id = $"ref_{r.Id}", DisplayOrder = itemOrder++ };
                item.SetValue("ReferenceName", r.ReferenceName, "Reference Name");
                item.SetValue("Company",       r.Company,       "Company");
                item.SetValue("Designation",   r.Designation,   "Designation");
                item.SetValue("Phone",         r.Phone,         "Phone", ResumeFieldType.Phone);
                item.SetValue("Email",         r.Email,         "Email", ResumeFieldType.Email);
                item.SetValue("Relationship",  r.Relationship,  "Relationship");
                item.SetValue("Notes",         r.Notes,         "Notes", ResumeFieldType.Textarea);
                section.Items.Add(item);
            }
            doc.Sections.Add(section);
        }

        return doc;
    }

    public async Task<bool> SaveResumeDocumentAsync(string userId, int? resumeId, ResumeDocument document, CancellationToken cancellationToken = default)
    {
        if (!resumeId.HasValue) return false;

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null) return false;

        var customResume = await _context.CandidateResumes
            .FirstOrDefaultAsync(r => r.Id == resumeId.Value && r.CandidateProfileId == profile.Id, cancellationToken);
            
        if (customResume != null)
        {
            customResume.DocumentData = System.Text.Json.JsonSerializer.Serialize(document, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }

    // ── Reference CRUD ────────────────────────────────────────────────────────

    public async Task<List<ReferenceViewModel>> GetReferencesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var profileId = await GetProfileIdAsync(userId, cancellationToken);
        if (profileId == null) return new();

        return await _context.CandidateReferences
            .Where(r => r.CandidateProfileId == profileId.Value)
            .AsNoTracking()
            .OrderBy(r => r.DisplayOrder)
            .Select(r => new ReferenceViewModel
            {
                Id            = r.Id,
                ReferenceName = r.ReferenceName,
                Company       = r.Company,
                Designation   = r.Designation,
                Phone         = r.Phone,
                Email         = r.Email,
                Relationship  = r.Relationship,
                Notes         = r.Notes,
                DisplayOrder  = r.DisplayOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferenceViewModel?> UpsertReferenceAsync(string userId, ReferenceViewModel model, CancellationToken cancellationToken = default)
    {
        var profileId = await GetProfileIdAsync(userId, cancellationToken);
        if (profileId == null) return null;

        CandidateReference entity;
        if (model.Id == 0)
        {
            entity = new CandidateReference { CandidateProfileId = profileId.Value };
            _context.CandidateReferences.Add(entity);
        }
        else
        {
            entity = await _context.CandidateReferences
                .FirstOrDefaultAsync(r => r.Id == model.Id && r.CandidateProfileId == profileId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Reference {model.Id} not found.");
        }

        entity.ReferenceName = model.ReferenceName;
        entity.Company       = model.Company;
        entity.Designation   = model.Designation;
        entity.Phone         = model.Phone;
        entity.Email         = model.Email;
        entity.Relationship  = model.Relationship;
        entity.Notes         = model.Notes;
        entity.DisplayOrder  = model.DisplayOrder;

        await _context.SaveChangesAsync(cancellationToken);

        model.Id = entity.Id;
        return model;
    }

    public async Task<bool> DeleteReferenceAsync(string userId, int referenceId, CancellationToken cancellationToken = default)
    {
        var profileId = await GetProfileIdAsync(userId, cancellationToken);
        if (profileId == null) return false;

        var entity = await _context.CandidateReferences
            .FirstOrDefaultAsync(r => r.Id == referenceId && r.CandidateProfileId == profileId.Value, cancellationToken);

        if (entity == null) return false;

        _context.CandidateReferences.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int?> GetProfileIdAsync(string userId, CancellationToken cancellationToken)
    {
        var id = await _context.CandidateProfiles
            .Where(p => p.UserId == userId)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return id;
    }
}
