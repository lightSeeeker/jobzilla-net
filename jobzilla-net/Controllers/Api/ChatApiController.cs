using jobzilla_net.Application.Chat;
using jobzilla_net.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace jobzilla_net.Controllers.Api;

[Route("api/chat")]
[ApiController]
[Authorize]
public class ChatApiController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatApiController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var conversations = await _chatService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var messages = await _chatService.GetMessagesAsync(id, userId);
        return Ok(messages);
    }

    [HttpPost("conversations/{id}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("Message cannot be empty.");

        try
        {
            var msg = await _chatService.SaveMessageAsync(id, userId, request.Body);

            // Fetch the conversation to find the other party
            var conversations = await _chatService.GetConversationsAsync(userId);
            var conv = conversations.FirstOrDefault(c => c.ConversationId == id);
            
            if (conv != null && !string.IsNullOrEmpty(conv.OtherPartyUserId))
            {
                // Send to the other party via SignalR
                var receiverDto = new jobzilla_net.Application.Chat.Dtos.MessageDto
                {
                    MessageId = msg.MessageId,
                    ConversationId = msg.ConversationId,
                    SenderUserId = msg.SenderUserId,
                    Body = msg.Body,
                    SentAtUtc = msg.SentAtUtc,
                    IsMine = false,
                    SenderName = conv.OtherPartyName, // When receiver gets it, the sender name is the other party name from their POV
                    SenderImageUrl = conv.OtherPartyImageUrl // But actually from our POV, it's 'Me' so the other party's POV is US
                };
                
                // More accurately, we send the exact msg, but set IsMine = false.
                msg.IsMine = false;
                
                await _hubContext.Clients.User(conv.OtherPartyUserId).SendAsync("ReceiveMessage", msg);
            }

            // Return true message to sender
            msg.IsMine = true;
            return Ok(msg);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public class SendMessageRequest
{
    public string Body { get; set; } = string.Empty;
}
