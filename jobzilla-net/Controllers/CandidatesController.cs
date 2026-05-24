using jobzilla_net.Application.Candidates;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Models.Candidates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Controllers;

/// <summary>
/// Handles public-facing candidate listing and detail pages.
/// No business logic — delegates entirely to <see cref="ICandidateService"/>.
/// </summary>
public sealed class CandidatesController : Controller
{
    private readonly ICandidateService _candidateService;
    private readonly IApplicationDbContext _db;

    public CandidatesController(ICandidateService candidateService, IApplicationDbContext db)
    {
        _candidateService = candidateService;
        _db = db;
    }

    /// <summary>
    /// GET /Candidates
    /// Public candidate listing with optional keyword, location,
    /// experience, and salary filters. Server-side paginated.
    /// </summary>
    public async Task<IActionResult> Index(
        CandidateSearchViewModel model,
        CancellationToken cancellationToken)
    {
        // Load filter dropdowns
        model.Categories = await _db.JobCategories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        model.Skills = await _db.Skills
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        model.Result = await _candidateService.SearchAsync(
            model.ToQuery(),
            cancellationToken);

        return View(model);
    }

    /// <summary>
    /// GET /Candidates/Details/{id}
    /// Public candidate profile detail page.
    /// Returns 404 if the profile does not exist or is soft-deleted.
    /// </summary>
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var candidate = await _candidateService.GetByIdAsync(id, cancellationToken);

        if (candidate is null)
            return NotFound();

        return View(candidate);
    }
}
