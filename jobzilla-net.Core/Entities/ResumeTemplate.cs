using jobzilla_net.Core.Common;
using jobzilla_net.Core.Enums;

namespace jobzilla_net.Core.Entities;

public class ResumeTemplate : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// System templates: Razor view name under ~/Views/Shared/ResumeTemplates.
    /// Custom templates: wwwroot-relative path to the uploaded HTML file.
    /// </summary>
    public string TemplateFilePath { get; set; } = string.Empty;
    public string? PreviewImagePath { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; }
    public bool IsPremium { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? TemplateType { get; set; }
    public ResumeTemplateSource Source { get; set; } = ResumeTemplateSource.System;
}
