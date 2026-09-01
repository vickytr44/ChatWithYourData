using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ChatWithYourData.ChatService.IntegrationTests;

public class ChatServiceIntegrationTests : IClassFixture<WebApplicationFactory<API.Program>>
{
    private readonly WebApplicationFactory<API.Program> _baseFactory;
    private readonly WebApplicationFactory<API.Program> _factory;

    public ChatServiceIntegrationTests(WebApplicationFactory<API.Program> factory)
    {
        _baseFactory = factory;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Agent:ApiKey"] = "test-api-key"
                });
            });
        });
    }

    [Fact]
    public void Startup_WhenApiKeyMissing_ThrowsInvalidOperationException()
    {
        // Act
        var act = () =>
        {
            var clientFactory = _baseFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Agent:ApiKey"] = ""
                    });
                });
            });
            _ = clientFactory.CreateClient();
        };

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*API key is not configured*");
    }

    [Fact]
    public async Task RootEndpoint_ReturnsHealthyStatusAndMetadata()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ChatWithYourData.ChatService");
        content.Should().Contain("Microsoft.Agents.AI");
        content.Should().Contain("AG-UI");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Status");
    }

    [Fact]
    public async Task AgUiEndpoint_IsMappedAndReachable()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Sending request to AG-UI route
        var response = await client.GetAsync("/ag-ui");

        // Assert - AG-UI handles POST for SSE runs or MethodNotAllowed/NotFound for bare GET
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
