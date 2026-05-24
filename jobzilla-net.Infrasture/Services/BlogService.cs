using jobzilla_net.Application.Blogs;
using jobzilla_net.Application.Blogs.Dtos;
using jobzilla_net.Application.Blogs.Queries;
using jobzilla_net.Application.Common;
using jobzilla_net.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services;

/// <summary>
/// EF Core implementation of <see cref="IBlogService"/>.
/// </summary>
public sealed class BlogService : IBlogService
{
    private readonly IApplicationDbContext _db;

    public BlogService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<BlogPostDto>> SearchAsync(
        BlogSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.BlogPosts
            .AsNoTracking()
            .Where(b => b.IsPublished && !b.IsDeleted);

        // ── Optional filters ─────────────────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim().ToLower();
            q = q.Where(b => 
                b.Title.ToLower().Contains(kw) || 
                (b.Excerpt != null && b.Excerpt.ToLower().Contains(kw)));
        }

        if (query.CategoryId.HasValue)
        {
            q = q.Where(b => b.CategoryId == query.CategoryId.Value);
        }

        // ── Count before paging ───────────────────────────────────────────────
        var totalCount = await q.CountAsync(cancellationToken);

        // ── Order + paginate ─────────────────────────────────────────────────
        var items = await q
            .OrderByDescending(b => b.PublishedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BlogPostDto
            {
                Id                = b.Id,
                Title             = b.Title,
                Slug              = b.Slug,
                Excerpt           = b.Excerpt,
                // Content is intentionally excluded in list views
                FeaturedImagePath = b.FeaturedImagePath,
                PublishedAtUtc    = b.PublishedAtUtc,
                AuthorName        = b.AuthorName,
                CategoryId        = b.CategoryId,
                CategoryName      = b.Category != null ? b.Category.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<BlogPostDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = query.Page,
            PageSize   = query.PageSize
        };
    }

    /// <inheritdoc/>
    public async Task<BlogPostDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _db.BlogPosts
            .AsNoTracking()
            .Where(b => b.Slug == slug && b.IsPublished && !b.IsDeleted)
            .Select(b => new BlogPostDto
            {
                Id                = b.Id,
                Title             = b.Title,
                Slug              = b.Slug,
                Excerpt           = b.Excerpt,
                Content           = b.Content, // Include full content here
                FeaturedImagePath = b.FeaturedImagePath,
                PublishedAtUtc    = b.PublishedAtUtc,
                AuthorName        = b.AuthorName,
                CategoryId        = b.CategoryId,
                CategoryName      = b.Category != null ? b.Category.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BlogPostDto>> GetLatestPostsAsync(
        int count = 3,
        CancellationToken cancellationToken = default)
    {
        return await _db.BlogPosts
            .AsNoTracking()
            .Where(b => b.IsPublished && !b.IsDeleted)
            .OrderByDescending(b => b.PublishedAtUtc)
            .Take(count)
            .Select(b => new BlogPostDto
            {
                Id                = b.Id,
                Title             = b.Title,
                Slug              = b.Slug,
                Excerpt           = b.Excerpt,
                FeaturedImagePath = b.FeaturedImagePath,
                PublishedAtUtc    = b.PublishedAtUtc,
                AuthorName        = b.AuthorName,
                CategoryId        = b.CategoryId,
                CategoryName      = b.Category != null ? b.Category.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
