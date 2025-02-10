using Microsoft.AspNetCore.Identity;

namespace SecureMessagingApp.Models;

public class User : IdentityUser<int>
{
    public override int Id { get; set; }
    public string PublicKey { get; set; }

    public ICollection<Message> SentMessages { get; set; }
    public ICollection<Message> ReceivedMessages { get; set; }
}