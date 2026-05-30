using jobzilla_net.Application.Chat.Dtos;

namespace jobzilla_net.Application.Chat;

public interface IChatService
{
    Task<int> StartOrGetConversationAsync(int jobApplicationId, string userId);
    Task<List<ConversationDto>> GetConversationsAsync(string userId);
    Task<List<MessageDto>> GetMessagesAsync(int conversationId, string userId);
    Task<MessageDto> SaveMessageAsync(int conversationId, string userId, string body);
}
