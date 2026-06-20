using jobzilla_net.Application.Admin.Dtos;

namespace jobzilla_net.Application.Admin;

public interface IHomePageContentService
{
    Task<List<HomePageContentDto>> GetAllAsync(CancellationToken ct = default);
    Task<HomePageContentDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<HomePageContentDto?> GetBySectionKeyAsync(string section, string key, CancellationToken ct = default);
    Task<int> CreateAsync(HomePageContentFormDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, HomePageContentFormDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<List<HomePageContentDto>> GetBySectionAsync(string section, CancellationToken ct = default);
}
