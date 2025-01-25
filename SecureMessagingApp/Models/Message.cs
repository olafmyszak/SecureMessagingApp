namespace SecureMessagingApp.Models;

public class Message
{
    public int Id { get; set; }
    public string EncryptedContent { get; set; }
    public DateTime Timestamp { get; set; }

    // Foreign keys
    public int SenderId { get; set; }
    public int RecipientId { get; set; }

    public User Sender { get; set; }
    public User Recipient { get; set; }
}