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
    private readonly ILogger<CandidateDashController> _logger;

    public CandidateDashController(
        ICandidateDashboardService dashboardService,
        jobzilla_net.Application.Resumes.Interfaces.IResumeParsingOrchestrator resumeOrchestrator,
        jobzilla_net.Application.Resumes.Interfaces.IResumeBuilderService resumeBuilderService,
        jobzilla_net.Application.Resumes.Interfaces.ITemplateRenderer templateRenderer,
        jobzilla_net.Application.Resumes.Interfaces.IResumeExportService resumeExportService,
        ILogger<CandidateDashController> logger)
    {
        _dashboardService = dashboardService;
        _resumeOrchestrator = resumeOrchestrator;
        _resumeBuilderService = resumeBuilderService;
        _templateRenderer = templateRenderer;
        _resumeExportService = resumeExportService;
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

        ViewBag.ResumeId = resumeId;
        var templates = await _resumeBuilderService.GetActiveTemplatesAsync();
        return View(templates);
    }

    [HttpGet]
    public async Task<IActionResult> PreviewTemplate(int id, int? resumeId = null)
    {
        var template = await _resumeBuilderService.GetTemplateByIdAsync(id);
        if (template == null) return NotFound("Template not found or inactive.");

        var model = await _resumeBuilderService.GetResumeDataAsync(GetUserId(), resumeId);

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
}

