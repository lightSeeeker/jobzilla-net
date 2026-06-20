using jobzilla_net.Application.Admin;
using jobzilla_net.Application.Admin.Dtos;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services;

public class HomePageContentService : IHomePageContentService
{
    private readonly IApplicationDbContext _context;

    public HomePageContentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HomePageContentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.HomePageContents
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Section)
            .ThenBy(x => x.DisplayOrder)
            .Select(x => new HomePageContentDto
            {
                Id = x.Id,
                Section = x.Section,
                Key = x.Key,
                Value = x.Value,
                ImageUrl = x.ImageUrl,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<HomePageContentDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.HomePageContents
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new HomePageContentDto
            {
                Id = x.Id,
                Section = x.Section,
                Key = x.Key,
                Value = x.Value,
                ImageUrl = x.ImageUrl,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<HomePageContentDto?> GetBySectionKeyAsync(string section, string key, CancellationToken ct = default)
    {
        return await _context.HomePageContents
            .AsNoTracking()
            .Where(x => x.Section == section && x.Key == key && x.IsActive && !x.IsDeleted)
            .Select(x => new HomePageContentDto
            {
                Id = x.Id,
                Section = x.Section,
                Key = x.Key,
                Value = x.Value,
                ImageUrl = x.ImageUrl,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<HomePageContentDto>> GetBySectionAsync(string section, CancellationToken ct = default)
    {
        return await _context.HomePageContents
            .AsNoTracking()
            .Where(x => x.Section == section && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new HomePageContentDto
            {
                Id = x.Id,
                Section = x.Section,
                Key = x.Key,
                Value = x.Value,
                ImageUrl = x.ImageUrl,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<int> CreateAsync(HomePageContentFormDto dto, CancellationToken ct = default)
    {
        var entity = new HomePageContent
        {
            Section = dto.Section,
            Key = dto.Key,
            Value = dto.Value,
            ImageUrl = dto.ImageUrl,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.HomePageContents.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(int id, HomePageContentFormDto dto, CancellationToken ct = default)
    {
        var entity = await _context.HomePageContents
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity == null)
            return false;

        entity.Section = dto.Section;
        entity.Key = dto.Key;
        entity.Value = dto.Value;
        entity.ImageUrl = dto.ImageUrl;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        _context.HomePageContents.Update(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.HomePageContents
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

        if (entity == null)
            return false;

        entity.IsDeleted = true;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        _context.HomePageContents.Update(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
