using System.Security.Claims;
using jobzilla_net.Application.Candidates;
using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Models.CandidateDash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jobzilla_net.Controllers;

[Authorize(Roles = "Candidate")]
public class CandidateDashController : Controller
{
    private readonly ICandidateDashboardService _dashboardService;
    private readonly jobzilla_net.Application.Resumes.Interfaces.IResumeParsingOrchestrator _resumeOrchestrator;
    private readonly jobzilla_net.Application.Resumes.Interfaces.IResumeBuilderService _resumeBuilderService;
    private readonly jobzilla_net.Application.Resumes.Interfaces.ITemplateRenderer _templateRenderer;
    private readonly jobzilla_net.Application.Resumes.Interfaces.IResumeExportService _resumeExportService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<CandidateDashController> _logger;

    public CandidateDashController(
        ICandidateDashboardService dashboardService,
        jobzilla_net.Application.Resumes.Interfaces.IResumeParsingOrchestrator resumeOrchestrator,
        jobzilla_net.Application.Resumes.Interfaces.IResumeBuilderService resumeBuilderService,
        jobzilla_net.Application.Resumes.Interfaces.ITemplateRenderer templateRenderer,
        jobzilla_net.Application.Resumes.Interfaces.IResumeExportService resumeExportService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<CandidateDashController> logger)
    {
        _dashboardService = dashboardService;
        _resumeOrchestrator = resumeOrchestrator;
        _resumeBuilderService = resumeBuilderService;
        _templateRenderer = templateRenderer;
        _resumeExportService = resumeExportService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new UnauthorizedAccessException("User ID claim not found.");
    }

    private string GetUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }

    // ── OVERVIEW ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var overview = await _dashboardService.GetDashboardOverviewAsync(GetUserId());

        var viewModel = new CandidateDashOverviewViewModel
        {
            Overview = overview
        };

        return View(viewModel);
    }

    // ── APPLIED JOBS ─────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> AppliedJobs(int page = 1)
    {
        const int pageSize = 10;
        var appliedJobs = await _dashboardService.GetAppliedJobsAsync(GetUserId(), page, pageSize);

        var viewModel = new CandidateDashAppliedJobsViewModel
        {
            AppliedJobs = appliedJobs
        };

        return View(viewModel);
    }

    // ── SAVED JOBS ───────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> SavedJobs(int page = 1)
    {
        const int pageSize = 10;
        var savedJobs = await _dashboardService.GetSavedJobsAsync(GetUserId(), page, pageSize);

        var viewModel = new CandidateDashSavedJobsViewModel
        {
            SavedJobs = savedJobs
        };

        return View(viewModel);
    }

    // ── PROFILE ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var profile = await _dashboardService.GetProfileAsync(GetUserId());

        // Populate email from Identity context since it is not part of CandidateProfile entity directly
        profile.Email = GetUserEmail();

        var viewModel = new CandidateDashProfileViewModel
        {
            Profile = profile
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CandidateDashProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            if (model.ProfileImage.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "File size must not exceed 5MB.");
                return View(model);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Invalid file format. Please upload an image (JPG, PNG, GIF).");
                return View(model);
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImage.CopyToAsync(fileStream);
            }

            model.Profile.ProfileImagePath = "/uploads/profiles/" + uniqueFileName;
        }

        var success = await _dashboardService.UpdateProfileAsync(GetUserId(), model.Profile);

        if (success)
        {
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        ModelState.AddModelError(string.Empty, "Failed to update profile.");
        return View(model);
    }

    // ── RESUMES ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Resumes()
    {
        var resumes = await _dashboardService.GetResumesAsync(GetUserId());

        var viewModel = new CandidateDashResumesViewModel
        {
            Resumes = resumes
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResume(IFormFile resumeFile, string title, bool isDefault)
    {
        if (resumeFile == null || resumeFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a valid file.";
            return RedirectToAction(nameof(Resumes));
        }

        // Validate file size (max 5 MB)
        const long maxFileSize = 5 * 1024 * 1024;
        if (resumeFile.Length > maxFileSize)
        {
            TempData["ErrorMessage"] = "File size cannot exceed 5MB.";
            return RedirectToAction(nameof(Resumes));
        }

        // Validate file extension
        var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
        var extension = Path.GetExtension(resumeFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            TempData["ErrorMessage"] = "Only PDF and DOCX files are allowed.";
            return RedirectToAction(nameof(Resumes));
        }

        // Validate MIME type
        var allowedMimeTypes = new[]
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/msword"
        };
        if (!allowedMimeTypes.Contains(resumeFile.ContentType))
        {
            TempData["ErrorMessage"] = "Invalid file content type.";
            return RedirectToAction(nameof(Resumes));
        }

        try
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a secure, unique file name to prevent path traversal and overwriting
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await resumeFile.CopyToAsync(fileStream);
            }

            var relativePath = $"/uploads/resumes/{uniqueFileName}";

            // Associate with Candidate profile
            var resumeDto = await _dashboardService.AddResumeAsync(GetUserId(), title, relativePath, isDefault);

            // Trigger Parsing Pipeline
            var parseSuccess = await _resumeOrchestrator.ParseAndSyncResumeAsync(GetUserId(), filePath);

            if (parseSuccess)
            {
                TempData["SuccessMessage"] = "Resume uploaded and parsed successfully! You can now edit the extracted fields.";
                return RedirectToAction(nameof(Builder), new { resumeId = resumeDto?.Id });
            }
            else
            {
                TempData["SuccessMessage"] = "Resume uploaded, but auto-parsing could not extract all fields. Please fill them manually.";
                return RedirectToAction(nameof(Builder), new { resumeId = resumeDto?.Id });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading resume for user {UserId}", GetUserId());
            TempData["ErrorMessage"] = "An error occurred while uploading the resume. Please try again.";
        }

        return RedirectToAction(nameof(Resumes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResumeScratch(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["ErrorMessage"] = "Please provide a resume title.";
            return RedirectToAction(nameof(Resumes));
        }

        try
        {
            var newResume = await _dashboardService.CreateScratchResumeAsync(GetUserId(), title);
            if (newResume != null)
            {
                TempData["SuccessMessage"] = "New resume created successfully!";
                return RedirectToAction(nameof(Builder), new { resumeId = newResume.Id });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating scratch resume for user {UserId}", GetUserId());
        }

        TempData["ErrorMessage"] = "Failed to create resume.";
        return RedirectToAction(nameof(Resumes));
    }

    // ── RESUME BUILDER (Parsed Data Editor) ──────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Builder(int? resumeId = null)
    {
        if (resumeId == null)
        {
            return RedirectToAction(nameof(Resumes));
        }

        ViewBag.ResumeId = resumeId;
        var model = await _resumeBuilderService.GetResumeDataAsync(GetUserId(), resumeId); // Legacy model used for fallback/templates
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Builder(jobzilla_net.Application.Resumes.ViewModels.ResumeExportViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the errors in the form.";
            return View(model);
        }

        var success = await _resumeBuilderService.UpdateResumeDataAsync(GetUserId(), model);

        if (success)
        {
            TempData["SuccessMessage"] = "Resume data updated successfully.";
            return RedirectToAction(nameof(Builder));
        }

        TempData["ErrorMessage"] = "Failed to update resume data.";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResume(int resumeId)
    {
        var success = await _dashboardService.DeleteResumeAsync(GetUserId(), resumeId);

        if (success)
        {
            TempData["SuccessMessage"] = "Resume deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Resume not found or could not be deleted.";
        }

        return RedirectToAction(nameof(Resumes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultResume(int resumeId)
    {
        var success = await _dashboardService.SetDefaultResumeAsync(GetUserId(), resumeId);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Active resume switched successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Resume not found or could not be updated.";
        }
        
        return RedirectToAction(nameof(Resumes));
    }

    // ── TEMPLATE SYSTEM ──────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Templates(int? resumeId = null)
    {
        if (resumeId == null)
        {
            return RedirectToAction(nameof(Resumes));
        }

        var profileId = await _dashboardService.GetProfileAsync(GetUserId()); // Ensure profile exists
        int? activeTemplateId = null;
        if (profileId != null)
        {
            var resumes = await _dashboardService.GetResumesAsync(GetUserId());
            var dbResume = resumes.FirstOrDefault(r => r.Id == resumeId.Value);
            activeTemplateId = dbResume?.TemplateId;
        }

        ViewBag.ResumeId = resumeId;
        ViewBag.ActiveTemplateId = activeTemplateId;
        ViewBag.PalettesJson = BuildPalettesJson();
        var templates = await _resumeBuilderService.GetActiveTemplatesAsync();
        return View(templates);
    }

    /// <summary>
    /// Returns palette definitions as a pre-serialized JSON string so the Razor view
    /// does not have to deal with complex nested generic types in @{ } code blocks.
    /// Shape: { [templateFilePathSlug]: [ { name, swatch, colors: {var:hex} } ] }
    /// </summary>
    private static string BuildPalettesJson()
    {
        var palettes = new Dictionary<string, object[]>
        {
            ["modernprofessional"] = new object[]
            {
                new { name = "Navy Teal",        swatch = "#48a9a6", colors = new { __mp_sidebar_bg = "#2b3a4a", __mp_accent = "#48a9a6" } },
                new { name = "Midnight Gold",    swatch = "#e2b04f", colors = new { __mp_sidebar_bg = "#1a1a2e", __mp_accent = "#e2b04f" } },
                new { name = "Forest Sage",      swatch = "#7bc8a4", colors = new { __mp_sidebar_bg = "#2d4a3e", __mp_accent = "#7bc8a4" } },
                new { name = "Slate Coral",      swatch = "#e8837a", colors = new { __mp_sidebar_bg = "#3d3d5c", __mp_accent = "#e8837a" } },
                new { name = "Charcoal Crimson", swatch = "#c0392b", colors = new { __mp_sidebar_bg = "#2c1810", __mp_accent = "#c0392b" } },
            },
            ["executivecorporate"] = new object[]
            {
                new { name = "Classic Black",  swatch = "#111111", colors = new { __ec_primary = "#111111", __ec_accent = "#444444" } },
                new { name = "Navy Blue",      swatch = "#2980b9", colors = new { __ec_primary = "#1a3a5c", __ec_accent = "#2980b9" } },
                new { name = "Burgundy",       swatch = "#8e1c1c", colors = new { __ec_primary = "#4a0e0e", __ec_accent = "#8e1c1c" } },
                new { name = "Forest",         swatch = "#2d7a3a", colors = new { __ec_primary = "#1a3a1e", __ec_accent = "#2d7a3a" } },
                new { name = "Warm Graphite",  swatch = "#7f8c8d", colors = new { __ec_primary = "#2c2c2c", __ec_accent = "#7f8c8d" } },
            },
            ["atsoptimized"] = new object[]
            {
                new { name = "Pure Black",  swatch = "#000000", colors = new { __ats_primary = "#000000" } },
                new { name = "Navy",        swatch = "#1a3a5c", colors = new { __ats_primary = "#1a3a5c" } },
                new { name = "Dark Green",  swatch = "#1a4a1e", colors = new { __ats_primary = "#1a4a1e" } },
                new { name = "Deep Red",    swatch = "#7a0000", colors = new { __ats_primary = "#7a0000" } },
                new { name = "Slate",       swatch = "#2c3e50", colors = new { __ats_primary = "#2c3e50" } },
            },
            ["creativedesigner"] = new object[]
            {
                new { name = "Sunset Orange",  swatch = "#ff7e5f", colors = new { __cd_header_from = "#ff7e5f", __cd_header_to = "#feb47b", __cd_accent = "#ff7e5f" } },
                new { name = "Purple Passion", swatch = "#8e44ad", colors = new { __cd_header_from = "#8e44ad", __cd_header_to = "#a569bd", __cd_accent = "#8e44ad" } },
                new { name = "Ocean Blue",     swatch = "#1a6fa8", colors = new { __cd_header_from = "#1a6fa8", __cd_header_to = "#3498db", __cd_accent = "#1a6fa8" } },
                new { name = "Emerald",        swatch = "#1a7a3a", colors = new { __cd_header_from = "#1a7a3a", __cd_header_to = "#27ae60", __cd_accent = "#1a7a3a" } },
                new { name = "Rose Gold",      swatch = "#c0392b", colors = new { __cd_header_from = "#c0392b", __cd_header_to = "#e74c3c", __cd_accent = "#c0392b" } },
            },
            ["technicaldeveloper"] = new object[]
            {
                new { name = "VS Dark",        swatch = "#4ec9b0", colors = new { __td_bg = "#1e1e1e", __td_name = "#ce9178", __td_keyword = "#569cd6", __td_fn = "#4ec9b0", __td_section = "#c586c0" } },
                new { name = "Monokai",        swatch = "#a6e22e", colors = new { __td_bg = "#272822", __td_name = "#f92672", __td_keyword = "#66d9ef", __td_fn = "#a6e22e", __td_section = "#fd971f" } },
                new { name = "Dracula",        swatch = "#50fa7b", colors = new { __td_bg = "#282a36", __td_name = "#ff79c6", __td_keyword = "#8be9fd", __td_fn = "#50fa7b", __td_section = "#bd93f9" } },
                new { name = "Solarized Dark", swatch = "#859900", colors = new { __td_bg = "#002b36", __td_name = "#268bd2", __td_keyword = "#2aa198", __td_fn = "#859900", __td_section = "#d33682" } },
                new { name = "Nord",           swatch = "#a3be8c", colors = new { __td_bg = "#2e3440", __td_name = "#88c0d0", __td_keyword = "#81a1c1", __td_fn = "#a3be8c", __td_section = "#b48ead" } },
            },
        };

        // Serialize with camelCase and then fix the key underscores back to dashes.
        // Anonymous type property names cannot contain dashes, so we used double-underscores
        // as a placeholder (e.g. __mp_accent → --mp-accent).
        var json = System.Text.Json.JsonSerializer.Serialize(palettes,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        // Replace __ prefix + underscores → CSS variable dashes: __mp_accent → --mp-accent
        json = System.Text.RegularExpressions.Regex.Replace(json, @"""__([\w_]+)"":", m =>
        {
            var cssVar = "--" + m.Groups[1].Value.Replace('_', '-');
            return $"\"{cssVar}\":";
        });

        return json;
    }

    [HttpGet]
    public async Task<IActionResult> PreviewTemplate(int id, int? resumeId = null)
    {
        var template = await _resumeBuilderService.GetTemplateByIdAsync(id);
        if (template == null) return NotFound("Template not found or inactive.");

        if (resumeId.HasValue)
        {
            await _resumeBuilderService.SetTemplateForResumeAsync(GetUserId(), resumeId.Value, id);
        }

        var model = await _resumeBuilderService.GetResumeDataAsync(GetUserId(), resumeId);
        
        model.TemplateId = id;
        model.ResumeId = resumeId;
        model.IsExport = false;

        var htmlContent = await _templateRenderer.RenderTemplateAsync(
            $"~/Views/Shared/ResumeTemplates/{template.TemplateFilePath}.cshtml",
            model);

        return Content(htmlContent, "text/html");
    }

    /// <summary>
    /// Read-only template render used exclusively for the in-card iframe preview on the Templates page.
    /// Intentionally does NOT call SetTemplateForResumeAsync so loading previews never changes
    /// the candidate's active template as a side effect.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SkeletonPreview(int id, int? resumeId = null)
    {
        var template = await _resumeBuilderService.GetTemplateByIdAsync(id);
        if (template == null) return NotFound();

        var model = await _resumeBuilderService.GetResumeDataAsync(GetUserId(), resumeId);

        model.TemplateId = id;
        model.ResumeId = resumeId;
        model.IsExport = false;

        var htmlContent = await _templateRenderer.RenderTemplateAsync(
            $"~/Views/Shared/ResumeTemplates/{template.TemplateFilePath}.cshtml",
            model);

        return Content(htmlContent, "text/html");
    }

    [HttpGet]
    public async Task<IActionResult> ExportTemplate(int id, int? resumeId = null)
    {
        var pdfBytes = await _resumeExportService.ExportResumeToPdfAsync(GetUserId(), id, resumeId); 
        
        if (pdfBytes == null)
        {
            TempData["ErrorMessage"] = "Failed to export resume. Please try again later.";
            return RedirectToAction(nameof(Templates));
        }

        var template = await _resumeBuilderService.GetTemplateByIdAsync(id);
        var fileName = $"Resume_{template?.Name ?? "Export"}_{DateTime.Now:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    // ── COLOR CUSTOMIZATION ───────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveColorSettings([FromBody] SaveColorSettingsRequest request)
    {
        if (request == null || request.Colors == null || request.ResumeId <= 0)
            return BadRequest(new { error = "Invalid request." });

        var ok = await _resumeBuilderService.SaveColorSettingsAsync(
            GetUserId(),
            request.ResumeId,
            request.TemplateId,
            request.Colors);

        return ok
            ? Ok(new { success = true })
            : StatusCode(500, new { error = "Failed to save color settings." });
    }

    // ── RESUME BUILDER JSON API ──────────────────────────────────────────────
    // All routes under /api/resume/* return JSON for the Builder SPA.

    [HttpGet("/api/resume/document")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ApiGetDocument([FromQuery] int? resumeId, CancellationToken ct)
    {
        try
        {
            var doc = await _resumeBuilderService.GetResumeDocumentAsync(GetUserId(), resumeId, ct);
            return Ok(doc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApiGetDocument failed for user {UserId}", GetUserId());
            return StatusCode(500, new { error = "Failed to load resume document." });
        }
    }

    [HttpPut("/api/resume/personal")]
    public async Task<IActionResult> ApiSavePersonal(
        [FromQuery] int? resumeId,
        [FromBody] jobzilla_net.Application.Resumes.Dtos.ResumeDocument body,
        CancellationToken ct)
    {
        if (body == null) return BadRequest(new { error = "Empty payload." });
        var ok = await _resumeBuilderService.SaveResumeDocumentAsync(GetUserId(), resumeId, body, ct);
        return ok ? Ok(new { success = true }) : StatusCode(500, new { error = "Save failed." });
    }

    [HttpPost("/api/resume/image")]
    public async Task<IActionResult> ApiUploadResumeImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "No file uploaded." });
        if (file.Length > 5 * 1024 * 1024) return BadRequest(new { error = "File size must not exceed 5MB." });
        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension)) return BadRequest(new { error = "Invalid file format." });

        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "resumes");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return Ok(new { url = "/uploads/resumes/" + uniqueFileName });
    }

    // ── Section items (per existing EF entities) ─────────────────────────────

    [HttpPost("/api/resume/experience")]
    public async Task<IActionResult> ApiAddExperience(
        [FromBody] jobzilla_net.Application.Resumes.ViewModels.CandidateExperienceViewModel body,
        CancellationToken ct)
    {
        if (body == null) return BadRequest();
        var ok = await _resumeBuilderService.UpdateResumeDataAsync(GetUserId(),
            new jobzilla_net.Application.Resumes.ViewModels.ResumeExportViewModel
            {
                Experiences = new() { body }
            }, ct);
        // Reload and return full document
        if (!ok) return StatusCode(500, new { error = "Save failed." });
        var doc = await _resumeBuilderService.GetResumeDocumentAsync(GetUserId(), null, ct);
        return Ok(new { success = true, document = doc });
    }

    // ── References CRUD ──────────────────────────────────────────────────────

    [HttpGet("/api/resume/references")]
    public async Task<IActionResult> ApiGetReferences(CancellationToken ct)
    {
        var refs = await _resumeBuilderService.GetReferencesAsync(GetUserId(), ct);
        return Ok(refs);
    }

    [HttpPost("/api/resume/references")]
    public async Task<IActionResult> ApiUpsertReference(
        [FromBody] jobzilla_net.Application.Resumes.ViewModels.ReferenceViewModel body,
        CancellationToken ct)
    {
        if (body == null || string.IsNullOrWhiteSpace(body.ReferenceName))
            return BadRequest(new { error = "ReferenceName is required." });

        var result = await _resumeBuilderService.UpsertReferenceAsync(GetUserId(), body, ct);
        return result != null ? Ok(result) : StatusCode(500, new { error = "Save failed." });
    }

    [HttpDelete("/api/resume/references/{id:int}")]
    public async Task<IActionResult> ApiDeleteReference(int id, CancellationToken ct)
    {
        var ok = await _resumeBuilderService.DeleteReferenceAsync(GetUserId(), id, ct);
        return ok ? Ok(new { success = true }) : NotFound(new { error = "Reference not found." });
    }

    [HttpPost]
    public async Task<IActionResult> ApplyJob([FromForm] int jobId, [FromForm] int resumeId, [FromForm] string? coverLetter)
    {
        var result = await _dashboardService.ApplyForJobAsync(GetUserId(), jobId, resumeId, coverLetter);
        
        if (result.Success)
        {
            return Json(new { success = true, message = result.Message });
        }
        else
        {
            return Json(new { success = false, message = result.Message });
        }
    }

    // ── CHAT ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Chat(int? conversationId)
    {
        ViewBag.ConversationId = conversationId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartChat(int applicationId, [FromServices] jobzilla_net.Application.Chat.IChatService chatService)
    {
        try
        {
            var conversationId = await chatService.StartOrGetConversationAsync(applicationId, GetUserId());
            return RedirectToAction(nameof(Chat), new { conversationId = conversationId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}

