using jobzilla_net.Application.Employers.Dtos;
using Microsoft.AspNetCore.Http;

namespace jobzilla_net.Models.Employer;

public class EmployerOverviewViewModel
{
    public EmployerboardOverviewDto Overview { get; set; } = new();
}

public class EmployerProfileViewModel
{
    public EmployerProfileDto Profile { get; set; } = new();
    public IFormFile? LogoImage { get; set; }
}

public class EmployerJobsViewModel
{
    public Application.Common.PagedResult<EmployerJobPostDto> PostedJobs { get; set; } = new();
}

public class EmployerApplicationsViewModel
{
    public Application.Common.PagedResult<EmployerJobApplicationDto> Applications { get; set; } = new();
}
