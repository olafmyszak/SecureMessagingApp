using SecureMessagingApp.Models;

namespace SecureMessagingApp.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(Message message);
    Task SendMessage(int recipientId, string encryptedContent);
    Task JoinConversation(int recipientId);
    Task LeaveConversation(int recipientId);
}