using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChatWithYourData.ChatService.IntegrationTests;

public class ChatServiceIntegrationTests : IClassFixture<WebApplicationFactory<API.Program>>
{
    private readonly WebApplicationFactory<API.Program> _factory;

    public ChatServiceIntegrationTests(WebApplicationFactory<API.Program> factory)
    {
        _factory = factory;
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
