using System.ComponentModel.DataAnnotations;

namespace jobzilla_net.Application.Blogs.Dtos;

public class BlogCreateUpdateDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Excerpt cannot exceed 500 characters.")]
    public string? Excerpt { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Display(Name = "Publish Immediately")]
    public bool IsPublished { get; set; }
}
