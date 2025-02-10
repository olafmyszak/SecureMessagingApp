using System.Net;
using System.Net.Http.Json;
using SecureMessagingApp.Dtos;

namespace SecureMessagingApp.IntegrationTests;

public class AuthTests(SecureMessagingAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new UserRegistrationDto { UserName = "newUser", Password = "P@ssw0rd!", PublicKey = "key1" };
        await Client.PostAsJsonAsync("/api/user/register", request);

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string token = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var request = new { UserName = "user", Password = "badpassword" };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new { UserName = "user", Password = "password" };
        await Client.PostAsJsonAsync("/api/auth/register", request);

        var badRequest = new { UserName = "user", Password = "badpassword" };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/login", badRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}