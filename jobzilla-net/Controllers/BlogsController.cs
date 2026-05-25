using jobzilla_net.Application.Blogs;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Models.Blogs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

    // ── CRUD OPERATIONS ──────────────────────────────────────────────────────

    private string GetUserId() => User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? string.Empty;
    private string GetUserName() => User.Identity?.Name ?? "Anonymous";

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public async Task<IActionResult> Manage(int page = 1)
    {
        const int pageSize = 10;
        var blogs = await _blogService.GetMyBlogsAsync(GetUserId(), page, pageSize);
        
        var viewModel = new BlogManageViewModel
        {
            Blogs = blogs
        };

        return View(viewModel);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new BlogFormViewModel { IsEditMode = false };
        await PopulateDropdownsAsync(model);
        return View("BlogForm", model);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View("BlogForm", model);
        }

        await _blogService.CreateBlogAsync(GetUserId(), GetUserName(), model.Blog);
        TempData["SuccessMessage"] = "Blog posted successfully.";
        return RedirectToAction(nameof(Manage));
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var blog = await _blogService.GetBlogForEditAsync(GetUserId(), id);
        if (blog == null) return NotFound();

        var model = new BlogFormViewModel
        {
            IsEditMode = true,
            BlogId = id,
            Blog = blog
        };
        await PopulateDropdownsAsync(model);
        return View("BlogForm", model);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogFormViewModel model)
    {
        if (id != model.BlogId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View("BlogForm", model);
        }

        var success = await _blogService.UpdateBlogAsync(GetUserId(), id, model.Blog);
        if (success)
        {
            TempData["SuccessMessage"] = "Blog updated successfully.";
            return RedirectToAction(nameof(Manage));
        }

        ModelState.AddModelError(string.Empty, "Failed to update blog.");
        await PopulateDropdownsAsync(model);
        return View("BlogForm", model);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _blogService.DeleteBlogAsync(GetUserId(), id);
        if (success)
        {
            TempData["SuccessMessage"] = "Blog deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete blog.";
        }
        return RedirectToAction(nameof(Manage));
    }

    private async Task PopulateDropdownsAsync(BlogFormViewModel model)
    {
        model.Categories = await _db.BlogCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();
    }
}
