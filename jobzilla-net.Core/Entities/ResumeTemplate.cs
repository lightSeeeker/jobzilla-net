using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class ResumeTemplate : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateFilePath { get; set; } = string.Empty;
    public string? PreviewImagePath { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; }
    public bool IsPremium { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? TemplateType { get; set; }
}
