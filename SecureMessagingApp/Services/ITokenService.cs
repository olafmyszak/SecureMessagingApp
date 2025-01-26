using SecureMessagingApp.Models;

namespace SecureMessagingApp.Services;

public interface ITokenService
{
    string GenerateJwtToken(User user);
}