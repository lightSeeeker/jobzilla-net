using Jobzilla.Domain.Enums;

namespace Jobzilla.Application.Jobs;

public class JobSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsFeatured { get; set; }
}
