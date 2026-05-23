using Jobzilla.Domain.Enums;

namespace Jobzilla.Application.Jobs;

public class JobSearchQuery
{
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public int? CategoryId { get; set; }
    public EmploymentType? EmploymentType { get; set; }
}
