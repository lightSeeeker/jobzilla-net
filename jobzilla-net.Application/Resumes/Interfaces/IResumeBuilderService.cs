using jobzilla_net.Application.Resumes.ViewModels;

namespace jobzilla_net.Application.Resumes.Interfaces;

public interface IResumeBuilderService
{
    Task<ResumeExportViewModel> GetResumeDataAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateResumeDataAsync(string userId, ResumeExportViewModel model, CancellationToken cancellationToken = default);
    Task<List<jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    Task<jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto?> GetTemplateByIdAsync(int templateId, CancellationToken cancellationToken = default);
}
