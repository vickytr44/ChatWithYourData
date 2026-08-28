using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ChatWithYourData.Gateway.IntegrationTests;

public class GatewayIntegrationTests : IClassFixture<WebApplicationFactory<ChatWithYourData.Gateway.Program>>
{
    private readonly HttpClient _client;

    public GatewayIntegrationTests(WebApplicationFactory<ChatWithYourData.Gateway.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootEndpoint_ReturnsSuccessAndMetadata()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ChatWithYourData.Gateway");
        content.Should().Contain("Healthy");
        content.Should().Contain("/graphql");
        content.Should().Contain("/graphql/mcp");
    }

    [Fact]
    public async Task GraphQLMcpEndpoint_ReturnsResponse()
    {
        // Act - MCP endpoint should be mapped and responsive
        var response = await _client.GetAsync("/graphql/mcp");

        // Assert - MCP endpoints respond to HTTP requests (e.g., MethodNotAllowed for GET or OK for SSE/post depending on transport)
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GraphQLEndpoint_ReturnsSuccess()
    {
        // Act - Query GraphQL endpoint
        var response = await _client.GetAsync("/graphql?query={__typename}");

        // Assert - Should return OK and GraphQL response
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GatewaySchema_ContainsAllMicroserviceFields()
    {
        // Act - Fetch the composed SDL from the Gateway
        var response = await _client.GetAsync("/graphql?sdl");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sdl = await response.Content.ReadAsStringAsync();
        sdl.Should().Contain("products");
        sdl.Should().Contain("customers");
        sdl.Should().Contain("vendors");
        sdl.Should().Contain("accounts");
        sdl.Should().Contain("invoices");

        // Stitched entity relationships across subgraphs
        sdl.Should().Contain("product: Product");
        sdl.Should().Contain("customer: Customer");
    }
}

