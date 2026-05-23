using Jobzilla.Application.Jobs;

namespace Jobzilla.Web.ViewModels.Jobs;

public class JobSearchViewModel
{
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public IReadOnlyList<JobSummaryDto> Jobs { get; set; } = Array.Empty<JobSummaryDto>();
}
