using SecureMessagingApp.Models;

namespace SecureMessagingApp.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(Message message);
    Task SendMessage(string encryptedContent, int recipientId);
}