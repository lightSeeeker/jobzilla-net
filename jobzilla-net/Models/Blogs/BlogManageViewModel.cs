using jobzilla_net.Application.Blogs.Dtos;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Models.Blogs;

public class BlogManageViewModel
{
    public PagedResult<BlogPostDto> Blogs { get; set; } = new PagedResult<BlogPostDto> { Items = new List<BlogPostDto>() };
}
