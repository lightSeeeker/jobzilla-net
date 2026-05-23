using System.Diagnostics;
using Jobzilla.Application.Common.Interfaces;
using Jobzilla.Application.Jobs;
using Jobzilla.Web.Models;
using Jobzilla.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jobzilla.Web.Controllers;

public class HomeController : Controller
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJobService _jobService;

    public HomeController(IApplicationDbContext dbContext, IJobService jobService)
    {
        _dbContext = dbContext;
        _jobService = jobService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new HomeIndexViewModel
        {
            FeaturedJobs = await _jobService.SearchAsync(new JobSearchQuery(), cancellationToken),
            Categories = await _dbContext.JobCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken)
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
