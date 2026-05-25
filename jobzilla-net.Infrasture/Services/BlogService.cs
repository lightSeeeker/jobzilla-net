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

    // ── CRUD OPERATIONS ──────────────────────────────────────────────────────

    public async Task<PagedResult<BlogPostDto>> GetMyBlogsAsync(string userId, int page, int pageSize)
    {
        var q = _db.BlogPosts
            .AsNoTracking()
            .Where(b => b.AuthorId == userId && !b.IsDeleted);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(b => b.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                CategoryName      = b.Category != null ? b.Category.Name : null,
                IsPublished       = b.IsPublished
            })
            .ToListAsync();

        return new PagedResult<BlogPostDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BlogCreateUpdateDto?> GetBlogForEditAsync(string userId, int blogId)
    {
        var blog = await _db.BlogPosts
            .FirstOrDefaultAsync(b => b.Id == blogId && b.AuthorId == userId && !b.IsDeleted);

        if (blog == null) return null;

        return new BlogCreateUpdateDto
        {
            Title = blog.Title,
            Content = blog.Content,
            Excerpt = blog.Excerpt,
            CategoryId = blog.CategoryId ?? 0,
            IsPublished = blog.IsPublished
        };
    }

    public async Task<int> CreateBlogAsync(string userId, string authorName, BlogCreateUpdateDto dto)
    {
        var blog = new jobzilla_net.Core.Entities.BlogPost
        {
            AuthorId = userId,
            AuthorName = authorName,
            Title = dto.Title,
            Slug = GenerateSlug(dto.Title),
            Content = dto.Content,
            Excerpt = dto.Excerpt,
            CategoryId = dto.CategoryId > 0 ? dto.CategoryId : null,
            IsPublished = dto.IsPublished,
            PublishedAtUtc = dto.IsPublished ? DateTime.UtcNow : null,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.BlogPosts.Add(blog);
        await _db.SaveChangesAsync();
        return blog.Id;
    }

    public async Task<bool> UpdateBlogAsync(string userId, int blogId, BlogCreateUpdateDto dto)
    {
        var blog = await _db.BlogPosts
            .FirstOrDefaultAsync(b => b.Id == blogId && b.AuthorId == userId && !b.IsDeleted);

        if (blog == null) return false;

        blog.Title = dto.Title;
        // Option to regenerate slug on title change, or keep original
        // blog.Slug = GenerateSlug(dto.Title);
        blog.Content = dto.Content;
        blog.Excerpt = dto.Excerpt;
        blog.CategoryId = dto.CategoryId > 0 ? dto.CategoryId : null;
        
        if (dto.IsPublished && !blog.IsPublished)
        {
            blog.PublishedAtUtc = DateTime.UtcNow;
        }
        else if (!dto.IsPublished)
        {
            blog.PublishedAtUtc = null;
        }
        
        blog.IsPublished = dto.IsPublished;

        _db.BlogPosts.Update(blog);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteBlogAsync(string userId, int blogId)
    {
        var blog = await _db.BlogPosts
            .FirstOrDefaultAsync(b => b.Id == blogId && b.AuthorId == userId);

        if (blog == null) return false;

        _db.BlogPosts.Remove(blog);
        return await _db.SaveChangesAsync() > 0;
    }

    private string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return Guid.NewGuid().ToString().Substring(0, 8);
        
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(":", "")
            .Replace(";", "")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("?", "")
            .Replace("!", "")
            .Replace("&", "and")
            .Replace("'", "");
            
        // Quick simple regex replacement could be better but this is minimal
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');

        return $"{slug}-{Guid.NewGuid().ToString().Substring(0, 6)}";
    }
}
