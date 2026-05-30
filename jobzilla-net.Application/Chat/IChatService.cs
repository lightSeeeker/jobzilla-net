using jobzilla_net.Application.Chat.Dtos;

namespace jobzilla_net.Application.Chat;

public interface IChatService
{
    Task<int> StartOrGetConversationAsync(int jobApplicationId, string userId);
    Task<List<ConversationDto>> GetConversationsAsync(string userId);
    Task<List<MessageDto>> GetMessagesAsync(int conversationId, string userId);
    Task<MessageDto> SaveMessageAsync(int conversationId, string userId, string body, string? attachmentUrl = null, string? attachmentName = null);
    Task<MessageDto> EditMessageAsync(int messageId, string userId, string newBody);
    Task DeleteMessageAsync(int messageId, string userId);
    Task DeleteConversationAsync(int conversationId, string userId);
}
