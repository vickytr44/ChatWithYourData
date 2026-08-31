using ChatWithYourData.ChatService.API.Configuration;
using ChatWithYourData.ChatService.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ChatWithYourData.ChatService.UnitTests;

public class McpToolProviderTests
{
    [Fact]
    public async Task GetToolsAsync_WhenGatewayUnreachable_ReturnsFallbackErpTools()
    {
        // Arrange
        var options = Options.Create(new AgentOptions
        {
            McpGatewayEndpoint = "http://127.0.0.1:59999/graphql/mcp" // Unreachable port
        });
        var logger = NullLogger<McpToolProvider>.Instance;
        await using var provider = new McpToolProvider(options, logger);

        // Act
        var tools = await provider.GetToolsAsync();

        // Assert
        tools.Should().NotBeNull();
        tools.Should().HaveCountGreaterThanOrEqualTo(4);
        tools.Select(t => t.Name).Should().Contain(new[] { "get_products", "get_sales_orders", "get_purchase_orders", "get_invoices" });
    }

    [Fact]
    public void AgentOptions_DefaultConfiguration_HasExpectedValues()
    {
        // Arrange & Act
        var options = new AgentOptions();

        // Assert
        options.Name.Should().Be("ChatWithYourDataERP");
        options.DisplayName.Should().Be("ERP Intelligent Assistant");
        options.Provider.Should().Be("GoogleGemini");
        options.Model.Should().Be("gemini-2.5-flash");
        options.Endpoint.Should().Be("https://generativelanguage.googleapis.com/v1beta/openai/");
        options.McpGatewayEndpoint.Should().Be("http://localhost:5000/graphql/mcp");
        options.AgUiEndpoint.Should().Be("/ag-ui");
        options.Instructions.Should().Contain("ChatWithYourData");
    }
}
