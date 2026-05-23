using System.ComponentModel.DataAnnotations;
using Jobzilla.Domain.Enums;

namespace Jobzilla.Web.ViewModels.Jobs;

public class PostJobViewModel
{
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int JobCategoryId { get; set; }

    [Required, StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

    [Range(0, 1000000)]
    public decimal? MinimumSalary { get; set; }

    [Range(0, 1000000)]
    public decimal? MaximumSalary { get; set; }

    [Required, StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Requirements { get; set; }

    [StringLength(4000)]
    public string? Responsibilities { get; set; }
}
