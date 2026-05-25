using jobzilla_net.Application.Blogs.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace jobzilla_net.Models.Blogs;

public class BlogFormViewModel
{
    public BlogCreateUpdateDto Blog { get; set; } = new();
    
    public bool IsEditMode { get; set; }
    
    public int? BlogId { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
}
