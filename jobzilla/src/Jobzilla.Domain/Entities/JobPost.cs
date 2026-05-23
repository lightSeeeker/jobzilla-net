using Jobzilla.Domain.Common;
using Jobzilla.Domain.Enums;

namespace Jobzilla.Domain.Entities;

public class JobPost : AuditableEntity
{
    public int EmployerProfileId { get; set; }
    public EmployerProfile? EmployerProfile { get; set; }
    public int JobCategoryId { get; set; }
    public JobCategory? JobCategory { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string Location { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public JobStatus Status { get; set; } = JobStatus.PendingApproval;
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsFeatured { get; set; }
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<SavedJob> SavedByCandidates { get; set; } = new List<SavedJob>();
    public ICollection<JobPostSkill> Skills { get; set; } = new List<JobPostSkill>();
}
