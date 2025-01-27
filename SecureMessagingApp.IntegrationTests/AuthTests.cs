using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SecureMessagingApp.IntegrationTests;

public class AuthTests(SecureMessagingAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly SecureMessagingAppFactory _factory = factory;

    [Fact]
    public async Task Register_ValidUser_ReturnsOkAndCreatesUser()
    {
        // Arrange
        var request = new { UserName = "newuser", Password = "P@ssw0rd!" };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Registration_SavesUserToDatabase()
    {
        // Arrange
        var testUser = new { UserName = "dbtestuser", Password = "P@ssw0rd!" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", testUser);

        // Assert
        response.EnsureSuccessStatusCode();

        // Query database directly
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserName == testUser.UserName);

        Assert.NotNull(userInDb);
        Assert.Equal(testUser.UserName, userInDb.UserName);
        Assert.NotNull(userInDb.PasswordHash);
    }

    [Fact]
    public async Task Registration_DuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var request = new { UserName = "existingUser", Password = "P@ssw0rd!" };
        await Client.PostAsJsonAsync("/api/auth/register", request);

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new { UserName = "user", Password = "P@ssw0rd!" };
        await Client.PostAsJsonAsync("/api/auth/register", request);

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