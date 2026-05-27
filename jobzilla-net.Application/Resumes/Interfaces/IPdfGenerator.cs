namespace jobzilla_net.Application.Resumes.Interfaces;

public interface IPdfGenerator
{
    Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent, CancellationToken cancellationToken = default);
}
