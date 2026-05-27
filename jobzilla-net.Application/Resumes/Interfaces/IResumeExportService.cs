namespace jobzilla_net.Application.Resumes.Interfaces;

public interface IResumeExportService
{
    Task<byte[]?> ExportResumeToPdfAsync(string userId, int templateId, CancellationToken cancellationToken = default);
}
