using jobzilla_net.Application.Employers.Dtos;

namespace jobzilla_net.Models.EmployerDash;

public class EmployerDashOverviewViewModel
{
    public EmployerDashboardOverviewDto Overview { get; set; } = new();
}

public class EmployerDashProfileViewModel
{
    public EmployerProfileDto Profile { get; set; } = new();
}

public class EmployerDashJobsViewModel
{
    public Application.Common.PagedResult<EmployerJobPostDto> PostedJobs { get; set; } = new();
}

public class EmployerDashApplicationsViewModel
{
    public Application.Common.PagedResult<EmployerJobApplicationDto> Applications { get; set; } = new();
}
