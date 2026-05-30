using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class Message : AuditableEntity
{
    public int ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; set; }

    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public DateTime? EditedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
}
