using jobzilla_net.Application.Chat;
using jobzilla_net.Application.Chat.Dtos;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services.Chat;

public class ChatService : IChatService
{
    private readonly IApplicationDbContext _context;

    public ChatService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> StartOrGetConversationAsync(int jobApplicationId, string userId)
    {
        var application = await _context.JobApplications
            .Include(a => a.JobPost)
            .ThenInclude(p => p!.EmployerProfile)
            .Include(a => a.CandidateProfile)
            .FirstOrDefaultAsync(a => a.Id == jobApplicationId);

        if (application == null)
            throw new ArgumentException("Job application not found.");

        // Verify the user is either the employer or the candidate
        bool isEmployer = application.JobPost?.EmployerProfile?.UserId == userId;
        bool isCandidate = application.CandidateProfile?.UserId == userId;

        if (!isEmployer && !isCandidate)
            throw new UnauthorizedAccessException("You are not authorized to chat for this application.");

        var candidateId = application.CandidateProfileId;
        var employerId = application.JobPost!.EmployerProfileId;

        // Find existing conversation between this candidate and employer
        var existingConv = await _context.Conversations
            .Include(c => c.JobApplication)
            .ThenInclude(a => a!.JobPost)
            .FirstOrDefaultAsync(c => c.JobApplication!.CandidateProfileId == candidateId && 
                                      c.JobApplication.JobPost!.EmployerProfileId == employerId);

        if (existingConv != null)
        {
            existingConv.IsDeletedByCandidate = false;
            existingConv.IsDeletedByEmployer = false;

            var systemMessageBody = $"[System]: Referenced job application for '{application.JobPost?.Title}'";
            var lastMessage = await _context.Messages
                .Where(m => m.ConversationId == existingConv.Id)
                .OrderByDescending(m => m.SentAtUtc)
                .FirstOrDefaultAsync();

            // Insert system marker if it's the first time discussing this job recently
            if (lastMessage == null || lastMessage.Body != systemMessageBody)
            {
                var sysMsg = new Message
                {
                    ConversationId = existingConv.Id,
                    SenderUserId = userId,
                    Body = systemMessageBody,
                    SentAtUtc = DateTime.UtcNow,
                    ReadAtUtc = DateTime.UtcNow // System messages are automatically "read"
                };
                _context.Messages.Add(sysMsg);
                await _context.SaveChangesAsync(CancellationToken.None);
            }

            return existingConv.Id;
        }

        var newConv = new Conversation
        {
            JobApplicationId = jobApplicationId,
            Subject = $"Chat between {application.JobPost.EmployerProfile?.CompanyName} and {application.CandidateProfile?.FullName}",
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Conversations.Add(newConv);
        await _context.SaveChangesAsync(CancellationToken.None);

        var firstSysMsg = new Message
        {
            ConversationId = newConv.Id,
            SenderUserId = userId,
            Body = $"[System]: Referenced job application for '{application.JobPost?.Title}'",
            SentAtUtc = DateTime.UtcNow,
            ReadAtUtc = DateTime.UtcNow
        };
        _context.Messages.Add(firstSysMsg);
        await _context.SaveChangesAsync(CancellationToken.None);

        return newConv.Id;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(string userId)
    {
        // Find conversations where the user is either the employer or candidate
        var conversations = await _context.Conversations
            .AsNoTracking()
            .Include(c => c.JobApplication)
                .ThenInclude(a => a!.JobPost)
                .ThenInclude(p => p!.EmployerProfile)
            .Include(c => c.JobApplication)
                .ThenInclude(a => a!.CandidateProfile)
            .Include(c => c.Messages)
            .Where(c => 
                (c.JobApplication!.JobPost!.EmployerProfile!.UserId == userId && !c.IsDeletedByEmployer) ||
                (c.JobApplication!.CandidateProfile!.UserId == userId && !c.IsDeletedByCandidate)
            )
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAtUtc) ?? c.CreatedAtUtc)
            .ToListAsync();

        var dtos = new List<ConversationDto>();

        foreach (var conv in conversations)
        {
            var isEmployer = conv.JobApplication?.JobPost?.EmployerProfile?.UserId == userId;
            var lastMessage = conv.Messages.OrderByDescending(m => m.SentAtUtc).FirstOrDefault();
            
            var dto = new ConversationDto
            {
                ConversationId = conv.Id,
                Subject = conv.Subject,
                JobApplicationId = conv.JobApplicationId,
                JobTitle = "", // Removed specific job title since the chat spans multiple jobs
                LastMessage = lastMessage?.Body,
                LastMessageAtUtc = lastMessage?.SentAtUtc,
                UnreadCount = conv.Messages.Count(m => m.SenderUserId != userId && m.ReadAtUtc == null)
            };

            if (isEmployer)
            {
                // The other party is the candidate
                dto.OtherPartyUserId = conv.JobApplication!.CandidateProfile!.UserId;
                dto.OtherPartyName = conv.JobApplication.CandidateProfile.FullName;
                dto.OtherPartyImageUrl = conv.JobApplication.CandidateProfile.ProfileImagePath;
            }
            else
            {
                // The other party is the employer
                dto.OtherPartyUserId = conv.JobApplication!.JobPost!.EmployerProfile!.UserId;
                dto.OtherPartyName = conv.JobApplication.JobPost.EmployerProfile.CompanyName;
                dto.OtherPartyImageUrl = conv.JobApplication.JobPost.EmployerProfile.LogoPath;
            }

            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<List<MessageDto>> GetMessagesAsync(int conversationId, string userId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.JobApplication)
                .ThenInclude(a => a!.JobPost)
                .ThenInclude(p => p!.EmployerProfile)
            .Include(c => c.JobApplication)
                .ThenInclude(a => a!.CandidateProfile)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            return new List<MessageDto>();

        bool isEmployer = conversation.JobApplication?.JobPost?.EmployerProfile?.UserId == userId;
        bool isCandidate = conversation.JobApplication?.CandidateProfile?.UserId == userId;

        if (!isEmployer && !isCandidate)
            return new List<MessageDto>();

        // Mark unread messages as read
        var unreadMessages = conversation.Messages.Where(m => m.SenderUserId != userId && m.ReadAtUtc == null).ToList();
        foreach (var msg in unreadMessages)
        {
            msg.ReadAtUtc = DateTime.UtcNow;
        }

        if (unreadMessages.Any())
        {
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        return conversation.Messages.OrderBy(m => m.SentAtUtc).Select(m => new MessageDto
        {
            MessageId = m.Id,
            ConversationId = m.ConversationId,
            SenderUserId = m.SenderUserId,
            Body = m.IsDeleted ? "This message was deleted." : m.Body,
            AttachmentUrl = m.IsDeleted ? null : m.AttachmentUrl,
            AttachmentName = m.IsDeleted ? null : m.AttachmentName,
            EditedAtUtc = m.EditedAtUtc,
            IsDeleted = m.IsDeleted,
            SentAtUtc = m.SentAtUtc,
            IsMine = m.SenderUserId == userId,
            // Simple mapping for names - in a real app, we'd lookup the sender's name dynamically or store it.
            // For now, if it's mine, sender name is "Me", else it's the other party.
            SenderName = m.SenderUserId == userId ? "Me" : 
                (isEmployer ? conversation.JobApplication!.CandidateProfile!.FullName : conversation.JobApplication!.JobPost!.EmployerProfile!.CompanyName),
            SenderImageUrl = m.SenderUserId == userId ? 
                (isEmployer ? conversation.JobApplication!.JobPost!.EmployerProfile!.LogoPath : conversation.JobApplication!.CandidateProfile!.ProfileImagePath) :
                (isEmployer ? conversation.JobApplication!.CandidateProfile!.ProfileImagePath : conversation.JobApplication!.JobPost!.EmployerProfile!.LogoPath)
        }).ToList();
    }

    public async Task<MessageDto> SaveMessageAsync(int conversationId, string userId, string body, string? attachmentUrl = null, string? attachmentName = null)
    {
        var conversation = await _context.Conversations
            .Include(c => c.JobApplication)
                .ThenInclude(a => a!.JobPost)
                .ThenInclude(p => p!.EmployerProfile)
            .Include(c => c.JobApplication)
                .ThenInclude(a => a!.CandidateProfile)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            throw new ArgumentException("Conversation not found.");

        bool isEmployer = conversation.JobApplication?.JobPost?.EmployerProfile?.UserId == userId;
        bool isCandidate = conversation.JobApplication?.CandidateProfile?.UserId == userId;

        if (!isEmployer && !isCandidate)
            throw new UnauthorizedAccessException("Not authorized.");

        conversation.IsDeletedByCandidate = false;
        conversation.IsDeletedByEmployer = false;

        var message = new Message
        {
            ConversationId = conversationId,
            SenderUserId = userId,
            Body = body,
            AttachmentUrl = attachmentUrl,
            AttachmentName = attachmentName,
            SentAtUtc = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(CancellationToken.None);

        return new MessageDto
        {
            MessageId = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            Body = message.Body,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentName = message.AttachmentName,
            SentAtUtc = message.SentAtUtc,
            IsMine = true,
            SenderName = "Me",
            SenderImageUrl = isEmployer ? conversation.JobApplication!.JobPost!.EmployerProfile!.LogoPath : conversation.JobApplication!.CandidateProfile!.ProfileImagePath
        };
    }

    public async Task<MessageDto> EditMessageAsync(int messageId, string userId, string newBody)
    {
        var msg = await _context.Messages
            .Include(m => m.Conversation)
                .ThenInclude(c => c!.JobApplication)
                    .ThenInclude(a => a!.JobPost)
                        .ThenInclude(p => p!.EmployerProfile)
            .Include(m => m.Conversation)
                .ThenInclude(c => c!.JobApplication)
                    .ThenInclude(a => a!.CandidateProfile)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (msg == null) throw new ArgumentException("Message not found.");
        if (msg.SenderUserId != userId) throw new UnauthorizedAccessException();
        if (msg.IsDeleted) throw new InvalidOperationException("Cannot edit deleted message.");
        if (msg.Body.StartsWith("[System]:")) throw new InvalidOperationException("Cannot edit system message.");

        msg.Body = newBody;
        msg.EditedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(CancellationToken.None);

        bool isEmployer = msg.Conversation!.JobApplication?.JobPost?.EmployerProfile?.UserId == userId;

        return new MessageDto
        {
            MessageId = msg.Id,
            ConversationId = msg.ConversationId,
            SenderUserId = msg.SenderUserId,
            Body = msg.Body,
            AttachmentUrl = msg.AttachmentUrl,
            AttachmentName = msg.AttachmentName,
            EditedAtUtc = msg.EditedAtUtc,
            IsDeleted = msg.IsDeleted,
            SentAtUtc = msg.SentAtUtc,
            IsMine = true,
            SenderName = "Me",
            SenderImageUrl = isEmployer ? msg.Conversation.JobApplication!.JobPost!.EmployerProfile!.LogoPath : msg.Conversation.JobApplication!.CandidateProfile!.ProfileImagePath
        };
    }

    public async Task DeleteMessageAsync(int messageId, string userId)
    {
        var msg = await _context.Messages.FindAsync(messageId);
        if (msg == null) throw new ArgumentException("Message not found.");
        if (msg.SenderUserId != userId) throw new UnauthorizedAccessException();
        if (msg.Body.StartsWith("[System]:")) throw new InvalidOperationException("Cannot delete system message.");

        msg.IsDeleted = true;
        msg.Body = ""; 
        msg.AttachmentUrl = null;
        msg.AttachmentName = null;
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DeleteConversationAsync(int conversationId, string userId)
    {
        var conv = await _context.Conversations
            .Include(c => c.JobApplication)
                .ThenInclude(a => a!.JobPost)
                .ThenInclude(p => p!.EmployerProfile)
            .Include(c => c.JobApplication)
                .ThenInclude(a => a!.CandidateProfile)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conv == null) throw new ArgumentException("Conversation not found.");

        bool isEmployer = conv.JobApplication?.JobPost?.EmployerProfile?.UserId == userId;
        bool isCandidate = conv.JobApplication?.CandidateProfile?.UserId == userId;

        if (isEmployer) conv.IsDeletedByEmployer = true;
        else if (isCandidate) conv.IsDeletedByCandidate = true;
        else throw new UnauthorizedAccessException();

        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
