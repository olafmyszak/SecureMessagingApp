using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SecureMessagingApp.Dtos;
using SecureMessagingApp.Models;

namespace SecureMessagingApp.IntegrationTests;

public class UserTests(SecureMessagingAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly SecureMessagingAppFactory _factory = factory;

    [Fact]
    public async Task Register_ValidUser_ReturnsOkAndCreatesUser()
    {
        // Arrange
        var request = new UserRegistrationDto { UserName = "newUser", Password = "P@ssw0rd!", PublicKey = "key1" };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/user/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Registration_SavesUserToDatabase()
    {
        // Arrange
        var request = new UserRegistrationDto { UserName = "newUser1", Password = "P@ssw0rd!", PublicKey = "key2" };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/user/register", request);

        // Assert
        response.EnsureSuccessStatusCode();

        // Query database directly
        using IServiceScope scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        User? userInDb = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

        Assert.NotNull(userInDb);
        Assert.Equal(request.UserName, userInDb.UserName);
        Assert.NotNull(userInDb.PasswordHash);
    }

    [Fact]
    public async Task Registration_DuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var request = new UserRegistrationDto { UserName = "newUser", Password = "P@ssw0rd!", PublicKey = "key3" };

        await Client.PostAsJsonAsync("/api/auth/register", request);

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/user/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}