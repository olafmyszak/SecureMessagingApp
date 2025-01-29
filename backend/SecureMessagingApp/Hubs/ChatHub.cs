using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SecureMessagingApp.Models;

namespace SecureMessagingApp.Hubs;

// [Authorize]
public sealed class ChatHub(AppDbContext context) : Hub<IChatClient>
{
    public async Task SendMessage(string encryptedContent, int recipientId)
    {
        int senderId = int.Parse(Context.UserIdentifier ?? throw new InvalidOperationException("Recipient id is null"));

        bool senderExists = await context.Users.AnyAsync(u => u.Id == senderId);
        if (!senderExists)
        {
            throw new KeyNotFoundException($"Recipient id {senderId} not found.");
        }

        if (string.IsNullOrWhiteSpace(encryptedContent))
        {
            throw new HubException("Invalid message content");
        }

        DateTime timestamp = DateTime.UtcNow;

        var message = new Message
        {
            EncryptedContent = encryptedContent,
            Timestamp = timestamp,
            SenderId = senderId,
            RecipientId = recipientId
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        await Clients.User(recipientId.ToString()).ReceiveMessage(message);
    }
}