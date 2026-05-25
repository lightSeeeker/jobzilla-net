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
    private readonly ILogger<CandidateDashController> _logger;

    public CandidateDashController(
        ICandidateDashboardService dashboardService,
        ILogger<CandidateDashController> logger)
    {
        _dashboardService = dashboardService;
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

        // Simplistic file upload for backend implementation phase
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(resumeFile.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await resumeFile.CopyToAsync(fileStream);
        }

        var relativePath = $"/uploads/resumes/{uniqueFileName}";
        
        await _dashboardService.AddResumeAsync(GetUserId(), title, relativePath, isDefault);

        TempData["SuccessMessage"] = "Resume uploaded successfully.";
        return RedirectToAction(nameof(Resumes));
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
            TempData["ErrorMessage"] = "Failed to delete resume or resume not found.";
        }

        return RedirectToAction(nameof(Resumes));
    }
}
