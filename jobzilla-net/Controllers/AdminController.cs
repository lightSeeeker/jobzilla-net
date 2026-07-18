using jobzilla_net.Application.Admin;
using jobzilla_net.Application.Admin.Dtos;
using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Jobs;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;
using jobzilla_net.Application.Resumes.ViewModels;
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
    private readonly IResumeHtmlComposer _resumeComposer;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminService adminService,
        IJobService jobService,
        IApplicationDbContext dbContext,
        IWebHostEnvironment webHostEnvironment,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IHomePageContentService homePageContentService,
        IResumeHtmlComposer resumeComposer,
        IPdfGenerator pdfGenerator,
        ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _jobService = jobService;
        _dbContext = dbContext;
        _webHostEnvironment = webHostEnvironment;
        _signInManager = signInManager;
        _userManager = userManager;
        _homePageContentService = homePageContentService;
        _resumeComposer = resumeComposer;
        _pdfGenerator = pdfGenerator;
        _logger = logger;
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

    // Only HTML templates (with placeholder tokens) can be uploaded from the admin
    // panel. They are rendered by string substitution, never executed, so untrusted
    // uploads are safe — unlike Razor views, which the 5 built-in templates use.
    private const string TemplateUploadDir = "uploads/resume-templates";
    private const string PreviewUploadDir = "uploads/resume-templates/previews";
    private const long MaxTemplateBytes = 2 * 1024 * 1024;   // 2 MB
    private const long MaxPreviewBytes = 2 * 1024 * 1024;    // 2 MB
    private static readonly string[] AllowedPreviewExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

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
            viewModel.ExistingTemplateFilePath = template.TemplateFilePath;
            viewModel.ExistingPreviewImagePath = template.PreviewImagePath;
            viewModel.Template = new AdminResumeTemplateFormDto
            {
                Name = template.Name,
                Description = template.Description,
                Category = template.Category,
                TemplateFilePath = template.TemplateFilePath,
                PreviewImagePath = template.PreviewImagePath,
                IsActive = template.IsActive,
                IsPremium = template.IsPremium,
                Price = template.Price,
                DiscountPrice = template.DiscountPrice,
                TemplateType = template.TemplateType,
                Source = template.Source
            };
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ResumeTemplateForm(AdminResumeTemplateFormViewModel viewModel)
    {
        // Built-in (System) templates are Razor views; only their metadata is editable.
        // Uploaded HTML files apply to Custom templates and new templates.
        var isSystem = viewModel.IsEditMode && viewModel.Template.Source == Core.Enums.ResumeTemplateSource.System;

        // A new HTML file is required when creating a Custom template; on edit the existing file is kept.
        if (!viewModel.IsEditMode && viewModel.TemplateFile == null)
            ModelState.AddModelError(nameof(viewModel.TemplateFile), "A template HTML file is required.");

        if (!isSystem && viewModel.TemplateFile != null)
        {
            if (Path.GetExtension(viewModel.TemplateFile.FileName).ToLowerInvariant() != ".html")
                ModelState.AddModelError(nameof(viewModel.TemplateFile), "Only .html template files are allowed.");
            if (viewModel.TemplateFile.Length > MaxTemplateBytes)
                ModelState.AddModelError(nameof(viewModel.TemplateFile), "Template file must be 2 MB or smaller.");
        }

        if (viewModel.PreviewImage != null)
        {
            if (!AllowedPreviewExtensions.Contains(Path.GetExtension(viewModel.PreviewImage.FileName).ToLowerInvariant()))
                ModelState.AddModelError(nameof(viewModel.PreviewImage), "Preview image must be JPG, PNG, GIF, or WEBP.");
            if (viewModel.PreviewImage.Length > MaxPreviewBytes)
                ModelState.AddModelError(nameof(viewModel.PreviewImage), "Preview image must be 2 MB or smaller.");
        }

        if (!ModelState.IsValid)
            return View(viewModel);

        try
        {
            // System templates keep their Razor view path; a new upload is ignored for them.
            var templateFilePath = (!isSystem && viewModel.TemplateFile != null)
                ? await SaveUploadAsync(viewModel.TemplateFile, TemplateUploadDir, ".html")
                : viewModel.ExistingTemplateFilePath ?? string.Empty;

            var previewImagePath = viewModel.PreviewImage != null
                ? await SaveUploadAsync(viewModel.PreviewImage, PreviewUploadDir, Path.GetExtension(viewModel.PreviewImage.FileName).ToLowerInvariant())
                : viewModel.ExistingPreviewImagePath;

            var dto = new AdminResumeTemplateFormDto
            {
                Name = viewModel.Template.Name,
                Description = viewModel.Template.Description,
                Category = viewModel.Template.Category,
                TemplateType = viewModel.Template.TemplateType,
                IsActive = viewModel.Template.IsActive,
                IsPremium = viewModel.Template.IsPremium,
                Price = viewModel.Template.IsPremium ? viewModel.Template.Price : 0m,
                DiscountPrice = viewModel.Template.IsPremium ? viewModel.Template.DiscountPrice : null,
                TemplateFilePath = templateFilePath,
                PreviewImagePath = previewImagePath,
                Source = isSystem ? Core.Enums.ResumeTemplateSource.System : Core.Enums.ResumeTemplateSource.Custom
            };

            if (viewModel.IsEditMode && viewModel.TemplateId.HasValue)
            {
                var result = await _adminService.UpdateResumeTemplateAsync(viewModel.TemplateId.Value, dto, GetUserId());
                if (result)
                {
                    TempData["SuccessMessage"] = "Template updated successfully";
                    return RedirectToAction(nameof(ResumeTemplates));
                }
                TempData["ErrorMessage"] = "Template not found.";
            }
            else
            {
                await _adminService.CreateResumeTemplateAsync(dto, GetUserId());
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

    private async Task<string> SaveUploadAsync(IFormFile file, string relativeDir, string extension)
    {
        var absoluteDir = Path.Combine(_webHostEnvironment.WebRootPath, relativeDir.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(absoluteDir);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteDir, fileName);

        await using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"{relativeDir}/{fileName}";
    }

    [HttpPost]
    public async Task<IActionResult> DeleteResumeTemplate(int id)
    {
        var template = await _adminService.GetResumeTemplateByIdAsync(id);
        if (template?.Source == Core.Enums.ResumeTemplateSource.System)
        {
            TempData["ErrorMessage"] = "Built-in templates cannot be deleted. Disable it instead.";
            return RedirectToAction(nameof(ResumeTemplates));
        }

        var result = await _adminService.DeleteResumeTemplateAsync(id, GetUserId());
        if (result)
        {
            DeleteWebRootFile(template?.TemplateFilePath);
            DeleteWebRootFile(template?.PreviewImagePath);
            TempData["SuccessMessage"] = "Template deleted successfully";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete template";
        }

        return RedirectToAction(nameof(ResumeTemplates));
    }

    private void DeleteWebRootFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;
        var absolute = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(absolute)) System.IO.File.Delete(absolute);
    }

    // Renders the full CV design for a template, filled with Lorem-Ipsum sample data,
    // so admins see how the layout looks. Returned as raw HTML for the preview iframe.
    [HttpGet]
    public async Task<IActionResult> TemplatePreview(int id)
    {
        var template = await _adminService.GetResumeTemplateByIdAsync(id);
        if (template == null || string.IsNullOrWhiteSpace(template.TemplateFilePath))
            return Content("<html><body></body></html>", "text/html");

        var dto = new ResumeTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            TemplateFilePath = template.TemplateFilePath,
            PreviewImagePath = template.PreviewImagePath,
            Source = template.Source
        };

        var html = await _resumeComposer.ComposeAsync(dto, BuildSampleResumeModel(id));
        return Content(html, "text/html");
    }

    // Isolated PDF test: renders one template (with sample data) straight through the
    // IPdfGenerator (in-process SelectPdf) and returns the PDF. Use it to
    // verify the PDF engine end-to-end without touching the real candidate export flow.
    [HttpGet]
    public async Task<IActionResult> TemplatePreviewPdf(int id, CancellationToken ct)
    {
        var template = await _adminService.GetResumeTemplateByIdAsync(id);
        if (template == null || string.IsNullOrWhiteSpace(template.TemplateFilePath))
            return NotFound("Template not found or has no layout file.");

        var dto = new ResumeTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            TemplateFilePath = template.TemplateFilePath,
            PreviewImagePath = template.PreviewImagePath,
            Source = template.Source
        };

        var model = BuildSampleResumeModel(id);
        model.IsExport = true;

        try
        {
            var html = await _resumeComposer.ComposeAsync(dto, model, ct);
            var pdf = await _pdfGenerator.GeneratePdfFromHtmlAsync(html, ct);
            return File(pdf, "application/pdf", $"Test_{template.Name}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Isolated PDF test failed for template {TemplateId}", id);
            return Content($"PDF generation failed: {ex.Message}", "text/plain");
        }
    }

    // Static placeholder CV data used only for admin template previews.
    private static ResumeExportViewModel BuildSampleResumeModel(int templateId)
    {
        const string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation.";

        return new ResumeExportViewModel
        {
            TemplateId = templateId,
            IsExport = false,
            Profile = new CandidateProfileDto
            {
                FullName = "Jordan Doe",
                ProfessionalTitle = "Senior Software Engineer",
                Email = "jordan.doe@example.com",
                PhoneNumber = "+1 555 012 3456",
                Location = "Doha, Qatar",
                Summary = lorem,
                ExperienceYears = 8
            },
            SocialLinks = new()
            {
                new() { PlatformName = "LinkedIn", Url = "https://linkedin.com/in/loremipsum" },
                new() { PlatformName = "GitHub", Url = "https://github.com/loremipsum" }
            },
            Skills = new() { "Lorem Ipsum", "Dolor Sit", "Amet Consectetur", "Adipiscing", "Tempor Labore", "Magna Aliqua" },
            Experiences = new()
            {
                new() { JobTitle = "Lead Developer", CompanyName = "Ipsum Technologies", Location = "Doha, QA", StartDate = new DateTime(2021, 1, 1), Description = lorem },
                new() { JobTitle = "Software Engineer", CompanyName = "Dolor Systems", Location = "Dubai, AE", StartDate = new DateTime(2017, 6, 1), EndDate = new DateTime(2020, 12, 1), Description = lorem }
            },
            Educations = new()
            {
                new() { InstitutionName = "Lorem University", Degree = "B.Sc.", FieldOfStudy = "Computer Science", StartDate = new DateTime(2013, 9, 1), EndDate = new DateTime(2017, 5, 1) }
            },
            Certifications = new()
            {
                new() { Name = "Certified Lorem Professional", IssuingOrganization = "Ipsum Institute", IssueDate = new DateTime(2022, 3, 1), CredentialUrl = "https://example.com/cert" }
            },
            Projects = new()
            {
                new() { Name = "Ipsum Platform", Description = lorem, ProjectUrl = "https://example.com", StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2023, 1, 1) }
            },
            References = new()
            {
                new() { ReferenceName = "Alex Amet", Designation = "Engineering Manager", Company = "Ipsum Technologies", Phone = "+1 555 987 6543", Email = "alex.amet@example.com" }
            }
        };
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
