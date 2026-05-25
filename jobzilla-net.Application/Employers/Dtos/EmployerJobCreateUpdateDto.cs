using System.ComponentModel.DataAnnotations;
using jobzilla_net.Core.Enums;

namespace jobzilla_net.Application.Employers.Dtos;

public class EmployerJobCreateUpdateDto
{
    public int JobCategoryId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public string? Requirements { get; set; }
    
    public string? Responsibilities { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;
    
    public EmploymentType EmploymentType { get; set; }
    
    public decimal? MinimumSalary { get; set; }
    
    public decimal? MaximumSalary { get; set; }
    
    public DateTime? ExpiresAtUtc { get; set; }
}
