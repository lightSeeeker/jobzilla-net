namespace jobzilla_net.Application.Chat.Dtos;

public class ConversationDto
{
    public int ConversationId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int? JobApplicationId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    
    public string OtherPartyUserId { get; set; } = string.Empty;
    public string OtherPartyName { get; set; } = string.Empty;
    public string? OtherPartyImageUrl { get; set; }
    
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAtUtc { get; set; }
    public int UnreadCount { get; set; }
}

public class MessageDto
{
    public int MessageId { get; set; }
    public int ConversationId { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderImageUrl { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
    public bool IsMine { get; set; }
}
