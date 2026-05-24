using jobzilla_net.Application.Blogs;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Models.Blogs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Controllers;

/// <summary>
/// Handles public-facing blog listing and detail pages.
/// </summary>
public sealed class BlogsController : Controller
{
    private readonly IBlogService _blogService;
    private readonly IApplicationDbContext _db;

    public BlogsController(IBlogService blogService, IApplicationDbContext db)
    {
        _blogService = blogService;
        _db = db;
    }

    /// <summary>
    /// GET /Blogs
    /// Public blog listing with optional keyword and category filters.
    /// </summary>
    public async Task<IActionResult> Index(
        BlogSearchViewModel model,
        CancellationToken cancellationToken)
    {
        // Load sidebar data
        model.Categories = await _db.BlogCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        model.LatestPosts = await _blogService.GetLatestPostsAsync(3, cancellationToken);

        // Load paginated results
        model.Result = await _blogService.SearchAsync(
            model.ToQuery(),
            cancellationToken);

        return View(model);
    }

    /// <summary>
    /// GET /Blogs/Details/{slug}
    /// Public blog post detail page.
    /// </summary>
    public async Task<IActionResult> Details(
        string id, // We use 'id' as the route parameter name, but it represents the slug
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var post = await _blogService.GetBySlugAsync(id, cancellationToken);

        if (post is null)
            return NotFound();

        var viewModel = new BlogDetailViewModel
        {
            Post = post,
            Categories = await _db.BlogCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken),
            LatestPosts = await _blogService.GetLatestPostsAsync(3, cancellationToken)
        };

        return View(viewModel);
    }
}
