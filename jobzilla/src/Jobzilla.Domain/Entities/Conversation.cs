using Jobzilla.Domain.Common;

namespace Jobzilla.Domain.Entities;

public class Conversation : AuditableEntity
{
    public string Subject { get; set; } = string.Empty;
    public int? JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
