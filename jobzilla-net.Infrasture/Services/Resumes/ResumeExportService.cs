using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class ResumeExportService : IResumeExportService
{
    private readonly IResumeBuilderService _builderService;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly ILogger<ResumeExportService> _logger;

    public ResumeExportService(
        IResumeBuilderService builderService,
        ITemplateRenderer templateRenderer,
        IPdfGenerator pdfGenerator,
        ILogger<ResumeExportService> logger)
    {
        _builderService = builderService;
        _templateRenderer = templateRenderer;
        _pdfGenerator = pdfGenerator;
        _logger = logger;
    }

    public async Task<byte[]?> ExportResumeToPdfAsync(string userId, int templateId, int? resumeId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting resume to PDF for user {UserId} using template {TemplateId}", userId, templateId);

        try
        {
            var template = await _builderService.GetTemplateByIdAsync(templateId, cancellationToken);
            if (template == null)
            {
                _logger.LogWarning("Template {TemplateId} not found or inactive.", templateId);
                return null;
            }

            var model = await _builderService.GetResumeDataAsync(userId, resumeId, cancellationToken);
            
            model.TemplateId = templateId;
            model.ResumeId = resumeId;
            model.IsExport = true;

            // Render HTML string
            var html = await _templateRenderer.RenderTemplateAsync(
                $"~/Views/Shared/ResumeTemplates/{template.TemplateFilePath}.cshtml", 
                model);

            // Generate PDF from HTML
            var pdfBytes = await _pdfGenerator.GeneratePdfFromHtmlAsync(html, cancellationToken);
            
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export resume to PDF for user {UserId}", userId);
            return null;
        }
    }
}
