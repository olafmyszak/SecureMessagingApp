namespace SecureMessagingApp.IntegrationTests;

public abstract class BaseIntegrationTest(SecureMessagingAppFactory factory) : IClassFixture<SecureMessagingAppFactory>
{
    protected readonly HttpClient Client = factory.CreateClient();
}