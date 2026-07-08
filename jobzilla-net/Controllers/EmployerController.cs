using System.Security.Claims;
using jobzilla_net.Application.Employers;
using jobzilla_net.Application.Employers.Dtos;
using jobzilla_net.Models.Employer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jobzilla_net.Controllers;

[Authorize(Roles = "Employer")]
public class EmployerController : Controller
{
    private readonly IEmployerboardService _dashboardService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EmployerController(IEmployerboardService dashboardService, IWebHostEnvironment webHostEnvironment)
    {
        _dashboardService = dashboardService;
        _webHostEnvironment = webHostEnvironment;
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
        
        var viewModel = new EmployerOverviewViewModel
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

        var viewModel = new EmployerProfileViewModel
        {
            Profile = profile
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(EmployerProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.LogoImage != null && model.LogoImage.Length > 0)
        {
            if (model.LogoImage.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "File size must not exceed 5MB.");
                return View(model);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(model.LogoImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Invalid file format. Please upload an image (JPG, PNG, GIF).");
                return View(model);
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "logos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generated name only — raw upload filenames can contain spaces/unicode
            // that break the served URL.
            string uniqueFileName = Guid.NewGuid().ToString("N") + extension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.LogoImage.CopyToAsync(fileStream);
            }

            model.Profile.LogoPath = "/uploads/logos/" + uniqueFileName;
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
        
        var viewModel = new EmployerJobsViewModel
        {
            PostedJobs = postedJobs
        };

        return View(viewModel);
    }

    // ── APPLICATIONS ─────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Applications(int page = 1, int? jobId = null)
    {
        const int pageSize = 10;
        var applications = await _dashboardService.GetApplicationsAsync(GetUserId(), page, pageSize, jobId);
        
        var viewModel = new EmployerApplicationsViewModel
        {
            Applications = applications
        };

        ViewBag.JobId = jobId;

        return View(viewModel);
    }

    [HttpGet("/api/employer/ats-score/{applicationId}")]
    public async Task<IActionResult> ApiGetAtsScore(int applicationId)
    {
        var score = await _dashboardService.CalculateAtsScoreAsync(GetUserId(), applicationId);
        return Ok(new { score });
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

    // ── JOBS CRUD ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> PostJob([FromServices] jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
    {
        var model = new EmployerJobFormViewModel { IsEditMode = false };
        await PopulateDropdownsAsync(model, dbContext);
        return View("JobForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostJob(EmployerJobFormViewModel model, [FromServices] jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
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

        var model = new EmployerJobFormViewModel
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
    public async Task<IActionResult> EditJob(int id, EmployerJobFormViewModel model, [FromServices] jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
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

    private async Task PopulateDropdownsAsync(EmployerJobFormViewModel model, jobzilla_net.Application.Common.Interfaces.IApplicationDbContext dbContext)
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
