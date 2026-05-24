using jobzilla_net.Application.Employers;
using jobzilla_net.Models.Employers;
using Microsoft.AspNetCore.Mvc;

namespace jobzilla_net.Controllers;

/// <summary>
/// Handles public-facing employer listing and detail pages.
/// No business logic — delegates entirely to <see cref="IEmployerService"/>.
/// </summary>
public sealed class EmployersController : Controller
{
    private readonly IEmployerService _employerService;

    public EmployersController(IEmployerService employerService)
    {
        _employerService = employerService;
    }

    /// <summary>
    /// GET /Employers
    /// Public employer listing with optional keyword, location,
    /// and industry filters. Server-side paginated.
    /// </summary>
    public async Task<IActionResult> Index(
        EmployerSearchViewModel model,
        CancellationToken cancellationToken)
    {
        model.Result = await _employerService.SearchAsync(
            model.ToQuery(),
            cancellationToken);

        return View(model);
    }

    /// <summary>
    /// GET /Employers/Details/{id}
    /// Public employer profile detail page.
    /// Returns 404 if the profile does not exist or is soft-deleted.
    /// </summary>
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var employer = await _employerService.GetByIdAsync(id, cancellationToken);

        if (employer is null)
            return NotFound();

        return View(employer);
    }
}
