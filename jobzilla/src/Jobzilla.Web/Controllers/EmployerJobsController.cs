using Jobzilla.Application.Common.Interfaces;
using Jobzilla.Domain.Entities;
using Jobzilla.Domain.Enums;
using Jobzilla.Infrastructure.Identity;
using Jobzilla.Web.ViewModels.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jobzilla.Web.Controllers;

[Authorize(Policy = "EmployerOnly")]
public class EmployerJobsController : Controller
{
    private readonly IApplicationDbContext _dbContext;

    public EmployerJobsController(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var jobs = await _dbContext.JobPosts
            .AsNoTracking()
            .Include(x => x.EmployerProfile)
            .Where(x => x.EmployerProfile != null && (User.IsInRole(AppRoles.Admin) || x.EmployerProfile.UserId == userId))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return View(jobs);
    }

    public IActionResult Create()
    {
        return View(new PostJobViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostJobViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var employer = await _dbContext.EmployerProfiles.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (employer is null)
        {
            employer = new EmployerProfile
            {
                UserId = userId,
                CompanyName = User.Identity?.Name ?? "New Employer",
                CreatedAtUtc = DateTime.UtcNow
            };
            _dbContext.EmployerProfiles.Add(employer);
        }

        _dbContext.JobPosts.Add(new JobPost
        {
            EmployerProfile = employer,
            JobCategoryId = model.JobCategoryId,
            Title = model.Title,
            Location = model.Location,
            EmploymentType = model.EmploymentType,
            MinimumSalary = model.MinimumSalary,
            MaximumSalary = model.MaximumSalary,
            Description = model.Description,
            Requirements = model.Requirements,
            Responsibilities = model.Responsibilities,
            Status = JobStatus.PendingApproval,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
