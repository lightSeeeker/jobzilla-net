using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.ViewModels;

namespace jobzilla_net.Application.Resumes.Interfaces;

public interface IResumeBuilderService
{
    // ── Legacy form-based API (kept for template export backward-compat) ─────
    Task<ResumeExportViewModel> GetResumeDataAsync(string userId, int? resumeId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateResumeDataAsync(string userId, ResumeExportViewModel model, CancellationToken cancellationToken = default);

    // ── Dynamic document API (new Builder UI) ────────────────────────────────
    Task<ResumeDocument> GetResumeDocumentAsync(string userId, int? resumeId = null, CancellationToken cancellationToken = default);
    Task<bool> SaveResumeDocumentAsync(string userId, int? resumeId, ResumeDocument document, CancellationToken cancellationToken = default);

    // ── Reference CRUD ───────────────────────────────────────────────────────
    Task<List<ReferenceViewModel>> GetReferencesAsync(string userId, CancellationToken cancellationToken = default);
    Task<ReferenceViewModel?> UpsertReferenceAsync(string userId, ReferenceViewModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteReferenceAsync(string userId, int referenceId, CancellationToken cancellationToken = default);

    // ── Template helpers ─────────────────────────────────────────────────────
    Task<List<jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    Task<jobzilla_net.Application.Resumes.Dtos.ResumeTemplateDto?> GetTemplateByIdAsync(int templateId, CancellationToken cancellationToken = default);
    Task<bool> SetTemplateForResumeAsync(string userId, int resumeId, int templateId, CancellationToken cancellationToken = default);
}

