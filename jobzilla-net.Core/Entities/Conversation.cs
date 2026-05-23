using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class Conversation : AuditableEntity
{
    public string Subject { get; set; } = string.Empty;
    public int? JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
