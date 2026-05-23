using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class ContentPage : AuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
}
