using Jobzilla.Domain.Common;

namespace Jobzilla.Domain.Entities;

public class Message : AuditableEntity
{
    public int ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; set; }
}
