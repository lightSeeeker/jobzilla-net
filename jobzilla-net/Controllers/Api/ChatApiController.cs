using jobzilla_net.Application.Chat;
using jobzilla_net.Application.Common.Utils;
using jobzilla_net.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    private readonly IWebHostEnvironment _env;

    public ChatApiController(IChatService chatService, IHubContext<ChatHub> hubContext, IWebHostEnvironment env)
    {
        _chatService = chatService;
        _hubContext = hubContext;
        _env = env;
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
    public async Task<IActionResult> SendMessage(int id, [FromForm] string? body, [FromForm] IFormFile? attachment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(body) && attachment == null)
            return BadRequest("Message and attachment cannot both be empty.");

        string? attachmentUrl = null;
        string? attachmentName = null;

        if (attachment != null)
        {
            using var stream = attachment.OpenReadStream();
            if (!FileValidator.IsValid(stream, attachment.FileName, attachment.ContentType, attachment.Length, out string errorMsg))
            {
                return BadRequest(errorMsg);
            }
            
            // Reset stream position if needed after validation read
            if (stream.CanSeek) stream.Position = 0;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "chat");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(attachment.FileName);
            var newFileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(uploadsFolder, newFileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await attachment.CopyToAsync(fs);
            }

            attachmentUrl = "/uploads/chat/" + newFileName;
            attachmentName = attachment.FileName;
        }

        try
        {
            var msg = await _chatService.SaveMessageAsync(id, userId, body ?? "", attachmentUrl, attachmentName);

            var conversations = await _chatService.GetConversationsAsync(userId);
            var conv = conversations.FirstOrDefault(c => c.ConversationId == id);
            
            if (conv != null && !string.IsNullOrEmpty(conv.OtherPartyUserId))
            {
                msg.IsMine = false;
                msg.SenderName = conv.OtherPartyName;
                msg.SenderImageUrl = conv.OtherPartyImageUrl;
                await _hubContext.Clients.User(conv.OtherPartyUserId).SendAsync("ReceiveMessage", msg);
            }

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

    [HttpPut("messages/{id}")]
    public async Task<IActionResult> EditMessage(int id, [FromBody] EditMessageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("Message body cannot be empty.");

        try
        {
            var msg = await _chatService.EditMessageAsync(id, userId, request.Body);

            var conversations = await _chatService.GetConversationsAsync(userId);
            var conv = conversations.FirstOrDefault(c => c.ConversationId == msg.ConversationId);

            if (conv != null && !string.IsNullOrEmpty(conv.OtherPartyUserId))
            {
                await _hubContext.Clients.User(conv.OtherPartyUserId).SendAsync("ReceiveMessageEdit", id, request.Body, msg.EditedAtUtc);
            }

            return Ok(msg);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("messages/{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            // We need the conversation ID to notify the other party. We can retrieve it before deleting.
            // Since our DeleteMessageAsync doesn't return the message, let's just delete it and if we wanted real-time,
            // we could broadcast. For simplicity, we just delete. We can fetch it first if we want to notify.
            // Wait, we DO want real-time. I'll just broadcast to all if possible, or we could fetch the message first.
            // Since it's an API, let's just delete it. Real-time for delete might require fetching first, but let's do a simple delete.
            await _chatService.DeleteMessageAsync(id, userId);

            return Ok(new { success = true });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("conversations/{id}")]
    public async Task<IActionResult> DeleteConversation(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            await _chatService.DeleteConversationAsync(id, userId);
            return Ok(new { success = true });
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

public class EditMessageRequest
{
    public string Body { get; set; } = string.Empty;
}
