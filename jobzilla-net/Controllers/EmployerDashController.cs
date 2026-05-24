using System.Security.Claims;
using jobzilla_net.Application.Employers;
using jobzilla_net.Application.Employers.Dtos;
using jobzilla_net.Models.EmployerDash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jobzilla_net.Controllers;

[Authorize(Roles = "Employer")]
public class EmployerDashController : Controller
{
    private readonly IEmployerDashboardService _dashboardService;

    public EmployerDashController(IEmployerDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
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
        
        var viewModel = new EmployerDashOverviewViewModel
        {
            Overview = overview
        };
        
        return View(viewModel);
    }

    // ── PROFILE ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var profile = await _dashboardService.GetProfileAsync(GetUserId());
        
        // Populate email from Identity context
        profile.Email = GetUserEmail();

        var viewModel = new EmployerDashProfileViewModel
        {
            Profile = profile
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(EmployerDashProfileViewModel model)
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

    // ── POSTED JOBS ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> PostedJobs(int page = 1)
    {
        const int pageSize = 10;
        var postedJobs = await _dashboardService.GetPostedJobsAsync(GetUserId(), page, pageSize);
        
        var viewModel = new EmployerDashJobsViewModel
        {
            PostedJobs = postedJobs
        };

        return View(viewModel);
    }

    // ── APPLICATIONS ─────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Applications(int page = 1)
    {
        const int pageSize = 10;
        var applications = await _dashboardService.GetApplicationsAsync(GetUserId(), page, pageSize);
        
        var viewModel = new EmployerDashApplicationsViewModel
        {
            Applications = applications
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateApplicationStatus(int applicationId, string newStatus)
    {
        var success = await _dashboardService.UpdateApplicationStatusAsync(GetUserId(), applicationId, newStatus);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Application status updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update application status.";
        }

        return RedirectToAction(nameof(Applications));
    }
}
