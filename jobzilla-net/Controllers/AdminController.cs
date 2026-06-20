using jobzilla_net.Application.Admin;
using jobzilla_net.Application.Admin.Dtos;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Jobs;
using jobzilla_net.Infrasture.Identity;
using jobzilla_net.Models.Account;
using jobzilla_net.Models.AdminDash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace jobzilla_net.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IJobService _jobService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHomePageContentService _homePageContentService;

    public AdminController(
        IAdminService adminService,
        IJobService jobService,
        IApplicationDbContext dbContext,
        IWebHostEnvironment webHostEnvironment,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IHomePageContentService homePageContentService)
    {
        _adminService = adminService;
        _jobService = jobService;
        _dbContext = dbContext;
        _webHostEnvironment = webHostEnvironment;
        _signInManager = signInManager;
        _userManager = userManager;
        _homePageContentService = homePageContentService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private string GetUserName() => User.Identity?.Name ?? "Admin";

    // Admin Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction(nameof(Index));

        var model = new LoginViewModel { ReturnUrl = returnUrl };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var returnUrl = model.ReturnUrl ?? Url.Action(nameof(Index)) ?? "/";
            return Redirect(returnUrl);
        }

        if (result.IsLockedOut)
            ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
        else if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(LoginWith2FA), new { returnUrl = model.ReturnUrl });
        else
            ModelState.AddModelError(string.Empty, "Invalid email or password");

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWith2FA(string? returnUrl = null)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
            return RedirectToAction(nameof(Login));

        return View(new LoginWith2FAViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2FA(LoginWith2FAViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
            return RedirectToAction(nameof(Login));

        var result = await _signInManager.TwoFactorSignInAsync("Email", model.TwoFactorCode, model.RememberMe, rememberClient: false);

        if (result.Succeeded)
        {
            var returnUrl = model.ReturnUrl ?? Url.Action(nameof(Index)) ?? "/";
            return Redirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid authentication code");
        return View(model);
    }

    // Dashboard
    [HttpGet]
    [Route("Dashboard/Index")]
    public async Task<IActionResult> Index()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        var activities = await _adminService.GetRecentActivitiesAsync();

        var viewModel = new AdminDashOverviewViewModel
        {
            Stats = stats,
            RecentActivities = activities
        };

        return View(viewModel);
    }

    [HttpGet("GetDashboardStats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Json(new
        {
            totalEmployers = stats.TotalEmployers,
            totalCandidates = stats.TotalCandidates,
            newUsersToday = stats.NewUsersToday,
            activeUsers = stats.ActiveUsers,
            totalJobsPosted = stats.TotalJobsPosted,
            activeJobs = stats.ActiveJobs,
            totalResumes = stats.TotalResumes,
            totalRevenue = stats.TotalRevenue.ToString("0"),
            expiredJobs = stats.ExpiredJobs
        });
    }

    // Employers
    [HttpGet]
    public async Task<IActionResult> Employers(string? keyword, string? status, int page = 1)
    {
        var query = new AdminEmployerQuery
        {
            Keyword = keyword,
            Status = status,
            Page = page,
            PageSize = 10
        };

        var result = await _adminService.GetEmployersAsync(query);

        var viewModel = new AdminEmployersViewModel
        {
            Employers = result,
            Keyword = keyword,
            Status = status
        };

        return View(viewModel);
    }

    [HttpGet("employers/{id}")]
    public async Task<IActionResult> EmployerDetail(int id)
    {
        var detail = await _adminService.GetEmployerDetailAsync(id);
        if (detail == null)
            return NotFound();

        return View(detail);
    }

    [HttpPost]
    public async Task<IActionResult> SetUserStatus(string userId, bool isActive)
    {
        var result = await _adminService.SetUserActiveStatusAsync(userId, isActive, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = isActive ? "User activated successfully" : "User deactivated successfully";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update user status";
        }

        return RedirectToAction(nameof(Employers));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEmployer(int id)
    {
        var result = await _adminService.DeleteEmployerAsync(id, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Employer deleted successfully";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete employer";
        }

        return RedirectToAction(nameof(Employers));
    }

    // Candidates
    [HttpGet]
    public async Task<IActionResult> Candidates(string? keyword, string? status, int page = 1)
    {
        var query = new AdminCandidateQuery
        {
            Keyword = keyword,
            Status = status,
            Page = page,
            PageSize = 10
        };

        var result = await _adminService.GetCandidatesAsync(query);

        var viewModel = new AdminCandidatesViewModel
        {
            Candidates = result,
            Keyword = keyword,
            Status = status
        };

        return View(viewModel);
    }

    [HttpGet("candidates/{id}")]
    public async Task<IActionResult> CandidateDetail(int id)
    {
        var detail = await _adminService.GetCandidateDetailAsync(id);
        if (detail == null)
            return NotFound();

        return View(detail);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCandidate(int id)
    {
        var result = await _adminService.DeleteCandidateAsync(id, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Candidate deleted successfully";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete candidate";
        }

        return RedirectToAction(nameof(Candidates));
    }

    // Resume Templates
    [HttpGet]
    public async Task<IActionResult> ResumeTemplates(int page = 1)
    {
        var result = await _adminService.GetResumeTemplatesAsync(page, 10);

        var viewModel = new AdminResumeTemplatesViewModel
        {
            Templates = result
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ResumeTemplateForm(int? id)
    {
        var viewModel = new AdminResumeTemplateFormViewModel();

        if (id.HasValue)
        {
            var template = await _adminService.GetResumeTemplateByIdAsync(id.Value);
            if (template == null)
                return NotFound();

            viewModel.TemplateId = id;
            viewModel.IsEditMode = true;
            viewModel.Template = new AdminResumeTemplateFormDto
            {
                Name = template.Name,
                Description = template.Description,
                Category = template.Category,
                TemplateFilePath = "",
                IsActive = template.IsActive,
                IsPremium = template.IsPremium,
                Price = template.Price,
                DiscountPrice = template.DiscountPrice,
                TemplateType = template.TemplateType
            };
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ResumeTemplateForm(AdminResumeTemplateFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        try
        {
            if (viewModel.IsEditMode && viewModel.TemplateId.HasValue)
            {
                var result = await _adminService.UpdateResumeTemplateAsync(viewModel.TemplateId.Value, viewModel.Template, GetUserId());
                if (result)
                {
                    TempData["SuccessMessage"] = "Template updated successfully";
                    return RedirectToAction(nameof(ResumeTemplates));
                }
            }
            else
            {
                var templateId = await _adminService.CreateResumeTemplateAsync(viewModel.Template, GetUserId());
                TempData["SuccessMessage"] = "Template created successfully";
                return RedirectToAction(nameof(ResumeTemplates));
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteResumeTemplate(int id)
    {
        var result = await _adminService.DeleteResumeTemplateAsync(id, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Template deleted successfully";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete template";
        }

        return RedirectToAction(nameof(ResumeTemplates));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleTemplateStatus(int id)
    {
        var result = await _adminService.ToggleResumeTemplateStatusAsync(id, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Template status updated";
        }
        return RedirectToAction(nameof(ResumeTemplates));
    }

    // Jobs
    [HttpGet]
    public async Task<IActionResult> Jobs(string? keyword, string? status, int? categoryId, int page = 1)
    {
        var categories = _dbContext.JobCategories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();

        var query = new AdminJobQuery
        {
            Keyword = keyword,
            Status = status,
            CategoryId = categoryId,
            Page = page,
            PageSize = 10
        };

        var result = await _adminService.GetJobsAsync(query);

        var viewModel = new AdminJobsViewModel
        {
            Jobs = result,
            Keyword = keyword,
            Status = status,
            CategoryId = categoryId,
            Categories = categories
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateJobStatus(int jobId, string newStatus)
    {
        var result = await _adminService.UpdateJobStatusAsync(jobId, newStatus, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Job status updated";
        }
        return RedirectToAction(nameof(Jobs));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleJobFeatured(int jobId)
    {
        var result = await _adminService.ToggleJobFeaturedAsync(jobId, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Job featured status updated";
        }
        return RedirectToAction(nameof(Jobs));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteJob(int jobId)
    {
        var result = await _adminService.DeleteJobAsync(jobId, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Job deleted successfully";
        }
        return RedirectToAction(nameof(Jobs));
    }

    // Applications
    [HttpGet]
    public async Task<IActionResult> Applications(int page = 1)
    {
        var result = await _adminService.GetApplicationsAsync(page, 10);

        var viewModel = new AdminApplicationsViewModel
        {
            Applications = result
        };

        return View(viewModel);
    }

    // Subscription Plans
    [HttpGet]
    public async Task<IActionResult> SubscriptionPlans()
    {
        var plans = await _adminService.GetSubscriptionPlansAsync();

        var viewModel = new AdminSubscriptionPlansViewModel
        {
            Plans = plans
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SubscriptionPlanForm(int? id)
    {
        var viewModel = new AdminSubscriptionPlanFormViewModel();

        if (id.HasValue)
        {
            var plans = await _adminService.GetSubscriptionPlansAsync();
            var plan = plans.FirstOrDefault(p => p.Id == id.Value);
            if (plan == null)
                return NotFound();

            viewModel.PlanId = id;
            viewModel.IsEditMode = true;
            viewModel.Plan = new AdminSubscriptionPlanFormDto
            {
                Name = plan.Name,
                Price = plan.Price,
                DurationDays = plan.DurationDays,
                JobPostLimit = plan.JobPostLimit,
                CanFeatureJobs = plan.CanFeatureJobs
            };
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SubscriptionPlanForm(AdminSubscriptionPlanFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        try
        {
            if (viewModel.IsEditMode && viewModel.PlanId.HasValue)
            {
                var result = await _adminService.UpdateSubscriptionPlanAsync(viewModel.PlanId.Value, viewModel.Plan, GetUserId());
                if (result)
                {
                    TempData["SuccessMessage"] = "Plan updated successfully";
                    return RedirectToAction(nameof(SubscriptionPlans));
                }
            }
            else
            {
                var planId = await _adminService.CreateSubscriptionPlanAsync(viewModel.Plan, GetUserId());
                TempData["SuccessMessage"] = "Plan created successfully";
                return RedirectToAction(nameof(SubscriptionPlans));
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSubscriptionPlan(int id)
    {
        var result = await _adminService.DeleteSubscriptionPlanAsync(id, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Plan deleted successfully";
        }
        return RedirectToAction(nameof(SubscriptionPlans));
    }

    // Payments
    [HttpGet]
    public async Task<IActionResult> Payments(string? keyword, string? status, int page = 1)
    {
        var query = new AdminPaymentQuery
        {
            Keyword = keyword,
            Status = status,
            Page = page,
            PageSize = 10
        };

        var payments = await _adminService.GetPaymentsAsync(query);
        var stats = await _adminService.GetPaymentStatsAsync();

        var viewModel = new AdminPaymentsViewModel
        {
            Payments = payments,
            Stats = stats,
            Keyword = keyword,
            Status = status
        };

        return View(viewModel);
    }

    // Content Pages
    [HttpGet]
    public async Task<IActionResult> ContentPages()
    {
        var pages = await _adminService.GetContentPagesAsync();

        var viewModel = new AdminContentPagesViewModel
        {
            Pages = pages
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> EditContentPage(string key)
    {
        var page = await _adminService.GetContentPageByKeyAsync(key);
        if (page == null)
            return NotFound();

        var viewModel = new AdminContentPageEditViewModel
        {
            Key = page.Key,
            Title = page.Title,
            HtmlContent = page.HtmlContent
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EditContentPage(AdminContentPageEditViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        var result = await _adminService.UpdateContentPageAsync(viewModel.Key, viewModel.Title, viewModel.HtmlContent, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Page updated successfully";
            return RedirectToAction(nameof(ContentPages));
        }

        TempData["ErrorMessage"] = "Failed to update page";
        return View(viewModel);
    }

    // Blogs
    [HttpGet]
    public async Task<IActionResult> Blogs(int page = 1)
    {
        var result = await _adminService.GetAllBlogsAsync(page, 10);

        var viewModel = new AdminBlogsViewModel
        {
            Blogs = result
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleBlogPublish(int id)
    {
        var result = await _adminService.AdminToggleBlogPublishAsync(id, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Blog publish status updated";
        }
        return RedirectToAction(nameof(Blogs));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBlog(int id)
    {
        var result = await _adminService.AdminDeleteBlogAsync(id, GetUserId());
        if (result)
        {
            TempData["SuccessMessage"] = "Blog deleted successfully";
        }
        return RedirectToAction(nameof(Blogs));
    }

    // Reports
    [HttpGet]
    public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var report = await _adminService.GetReportAsync(from, to);

        var viewModel = new AdminReportsViewModel
        {
            Report = report,
            FromDate = from,
            ToDate = to
        };

        return View(viewModel);
    }

    // Audit Log
    [HttpGet]
    public async Task<IActionResult> AuditLog(int page = 1)
    {
        var result = await _adminService.GetAuditLogsAsync(page, 20);

        var viewModel = new AdminAuditLogViewModel
        {
            Logs = result
        };

        return View(viewModel);
    }

    // Home Page Content Management
    [HttpGet]
    public async Task<IActionResult> HomePageContent(string? section = null)
    {
        var items = section != null
            ? await _homePageContentService.GetBySectionAsync(section)
            : await _homePageContentService.GetAllAsync();

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> EditHomePageContent(int? id)
    {
        HomePageContentDto? item = null;
        if (id.HasValue)
        {
            item = await _homePageContentService.GetByIdAsync(id.Value);
            if (item == null)
                return NotFound();
        }

        var viewModel = new AdminHomePageContentFormViewModel
        {
            Item = item,
            IsEditMode = id.HasValue
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditHomePageContent(HomePageContentFormDto model, int? id)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = new AdminHomePageContentFormViewModel
            {
                Item = id.HasValue ? await _homePageContentService.GetByIdAsync(id.Value) : null,
                IsEditMode = id.HasValue
            };
            return View(viewModel);
        }

        if (id.HasValue)
        {
            await _homePageContentService.UpdateAsync(id.Value, model);
        }
        else
        {
            await _homePageContentService.CreateAsync(model);
        }

        return RedirectToAction(nameof(HomePageContent));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteHomePageContent(int id)
    {
        await _homePageContentService.DeleteAsync(id);
        return RedirectToAction(nameof(HomePageContent));
    }
}
