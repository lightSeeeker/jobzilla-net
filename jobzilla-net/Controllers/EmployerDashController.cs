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
    // ── JOBS CRUD ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> PostJob([FromServices] jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
    {
        var model = new EmployerDashJobFormViewModel { IsEditMode = false };
        await PopulateDropdownsAsync(model, dbContext);
        return View("JobForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostJob(EmployerDashJobFormViewModel model, [FromServices] jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model, dbContext);
            return View("JobForm", model);
        }

        var jobId = await _dashboardService.CreateJobAsync(GetUserId(), model.Job);
        TempData["SuccessMessage"] = "Job posted successfully.";
        return RedirectToAction(nameof(PostedJobs));
    }

    [HttpGet]
    public async Task<IActionResult> EditJob(int id, [FromServices] jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
    {
        var job = await _dashboardService.GetJobForEditAsync(GetUserId(), id);
        if (job == null) return NotFound();

        var model = new EmployerDashJobFormViewModel
        {
            IsEditMode = true,
            JobId = id,
            Job = job
        };
        await PopulateDropdownsAsync(model, dbContext);
        return View("JobForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(int id, EmployerDashJobFormViewModel model, [FromServices] jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
    {
        if (id != model.JobId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model, dbContext);
            return View("JobForm", model);
        }

        var success = await _dashboardService.UpdateJobAsync(GetUserId(), id, model.Job);
        if (success)
        {
            TempData["SuccessMessage"] = "Job updated successfully.";
            return RedirectToAction(nameof(PostedJobs));
        }

        ModelState.AddModelError(string.Empty, "Failed to update job.");
        await PopulateDropdownsAsync(model, dbContext);
        return View("JobForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var success = await _dashboardService.DeleteJobAsync(GetUserId(), id);
        if (success)
        {
            TempData["SuccessMessage"] = "Job deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete job.";
        }
        return RedirectToAction(nameof(PostedJobs));
    }

    private async Task PopulateDropdownsAsync(EmployerDashJobFormViewModel model, jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
    {
        model.Categories = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            System.Linq.Queryable.Select(dbContext.JobCategories, c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }));
            
        model.EmploymentTypes = System.Enum.GetValues<jobzilla_net.Core.Enums.EmploymentType>()
            .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem 
            { 
                Value = e.ToString(), 
                Text = e.ToString() 
            })
            .ToList();
    }
}
