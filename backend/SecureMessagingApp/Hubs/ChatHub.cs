using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SecureMessagingApp.Models;

namespace SecureMessagingApp.Hubs;

[Authorize]
public sealed class ChatHub(AppDbContext context) : Hub<IChatClient>
{
    public async Task SendMessage(int recipientId, string encryptedContent)
    {
        bool recipientExists = await context.Users.AnyAsync(u => u.Id == recipientId);
        if (!recipientExists)
        {
            throw new HubException($"Recipient id {recipientId} not found.");
        }

        int senderId = int.Parse(Context.UserIdentifier ?? throw new InvalidOperationException("Sender id is null"));

        bool senderExists = await context.Users.AnyAsync(u => u.Id == senderId);
        if (!senderExists)
        {
            throw new HubException($"Sender id {senderId} not found.");
        }

        if (string.IsNullOrWhiteSpace(encryptedContent))
        {
            throw new HubException("Invalid message content");
        }

        var message = new Message
        {
            EncryptedContent = encryptedContent,
            Timestamp = DateTime.UtcNow,
            SenderId = senderId,
            RecipientId = recipientId
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();

        await Clients.User(recipientId.ToString()).ReceiveMessage(message);
    }

    public async Task JoinConversation(int recipientId)
    {
        bool recipientExists = await context.Users.AnyAsync(u => u.Id == recipientId);
        if (!recipientExists)
        {
            throw new HubException($"Recipient id {recipientId} not found.");
        }

        int senderId = int.Parse(Context.UserIdentifier ?? throw new InvalidOperationException("Sender id is null"));

        bool senderExists = await context.Users.AnyAsync(u => u.Id == senderId);
        if (!senderExists)
        {
            throw new HubException($"Sender id {senderId} not found.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroupName(senderId, recipientId));
    }

    public async Task LeaveConversation(int recipientId)
    {
        int senderId = int.Parse(Context.UserIdentifier ?? throw new InvalidOperationException("Sender id is null"));

        bool senderExists = await context.Users.AnyAsync(u => u.Id == senderId);
        if (!senderExists)
        {
            throw new HubException($"Sender id {senderId} not found.");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetConversationGroupName(senderId, recipientId));
    }

    private static string GetConversationGroupName(int userId1, int userId2)
    {
        int bigger = int.Max(userId1, userId2);
        int smaller = int.Min(userId1, userId2);

        return $"conversation_{bigger}-{smaller}";
    }
}