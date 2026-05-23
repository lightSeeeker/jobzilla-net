using Jobzilla.Application.Jobs;
using Jobzilla.Domain.Entities;

namespace Jobzilla.Web.ViewModels.Home;

public class HomeIndexViewModel
{
    public IReadOnlyList<JobSummaryDto> FeaturedJobs { get; set; } = Array.Empty<JobSummaryDto>();
    public IReadOnlyList<JobCategory> Categories { get; set; } = Array.Empty<JobCategory>();
}
