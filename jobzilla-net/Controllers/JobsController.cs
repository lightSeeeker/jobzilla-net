using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Jobs;
using jobzilla_net.Models.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Controllers;

/// <summary>
/// Handles public job listing and detail pages.
/// Thin controller: delegates all business logic to <see cref="IJobService"/>.
/// </summary>
public sealed class JobsController : Controller
{
    private readonly IJobService _jobService;
    private readonly IApplicationDbContext _db;

    public JobsController(IJobService jobService, IApplicationDbContext db)
    {
        _jobService = jobService;
        _db         = db;
    }

    // GET /Jobs  OR  /Jobs?keyword=...&location=...&categoryId=...&page=2
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] JobSearchViewModel model,
        CancellationToken cancellationToken)
    {
        // Load categories for sidebar filter dropdown (AsNoTracking, ordered)
        model.Categories = await _db.JobCategories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        // Delegate search + pagination to Application layer
        model.Result = await _jobService.SearchAsync(model.ToQuery(), cancellationToken);

        return View(model);
    }

    // GET /Jobs/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetByIdAsync(id, cancellationToken);

        if (job is null)
            return NotFound();

        return View(job);
    }
}
