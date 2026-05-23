using Jobzilla.Application.Jobs;
using Jobzilla.Web.ViewModels.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace Jobzilla.Web.Controllers;

public class JobsController : Controller
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    public async Task<IActionResult> Index([FromQuery] JobSearchViewModel request, CancellationToken cancellationToken)
    {
        request.Jobs = await _jobService.SearchAsync(new JobSearchQuery
        {
            Keyword = request.Keyword,
            Location = request.Location
        }, cancellationToken);

        return View(request);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetSummaryAsync(id, cancellationToken);
        if (job is null)
        {
            return NotFound();
        }

        return View(job);
    }
}
