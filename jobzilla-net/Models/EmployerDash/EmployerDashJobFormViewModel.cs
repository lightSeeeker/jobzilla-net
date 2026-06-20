using jobzilla_net.Application.Employers.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace jobzilla_net.Models.Employer;

public class EmployerJobFormViewModel
{
    public EmployerJobCreateUpdateDto Job { get; set; } = new();
    
    /// <summary>
    /// Determines whether the view is operating in Edit or Create mode.
    /// </summary>
    public bool IsEditMode { get; set; }
    
    /// <summary>
    /// When in Edit mode, holds the ID of the job being edited.
    /// </summary>
    public int? JobId { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> EmploymentTypes { get; set; } = new();
}
