using jobzilla_net.Application.Candidates;
using jobzilla_net.Models.Candidates;
using Microsoft.AspNetCore.Mvc;

namespace jobzilla_net.Controllers;

/// <summary>
/// Handles public-facing candidate listing and detail pages.
/// No business logic — delegates entirely to <see cref="ICandidateService"/>.
/// </summary>
public sealed class CandidatesController : Controller
{
    private readonly ICandidateService _candidateService;

    public CandidatesController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
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
