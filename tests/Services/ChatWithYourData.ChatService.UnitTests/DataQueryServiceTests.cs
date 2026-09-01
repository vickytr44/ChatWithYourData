using System.Text.Json;
using ChatWithYourData.ChatService.API.Models;
using ChatWithYourData.ChatService.API.Services;
using FluentAssertions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ChatWithYourData.ChatService.UnitTests;

public class DataQueryServiceTests
{
    [Fact]
    public async Task QueryAsync_WhenAgentReturnsStructuredOutput_NormalizesIntoTables()
    {
        // Arrange
        var mockChatClient = new Mock<IChatClient>();

        var sampleAgentJson = """
            {
              "summary": "Found 2 purchase orders with line items.",
              "primaryEntityName": "Purchase Orders",
              "success": true,
              "data": "[{\"poNumber\":\"PO-9921\",\"totalAmount\":12500.00,\"status\":\"pending_approval\",\"lines\":[{\"sku\":\"RAW-401\",\"name\":\"Silicon Wafers\",\"quantity\":100,\"unitPrice\":125.00}]}]"
            }
            """;

        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, sampleAgentJson)));

        var agent = new ChatClientAgent(mockChatClient.Object, name: "TestAgent");
        var service = new DataQueryService(agent, NullLogger<DataQueryService>.Instance);

        // Act
        var result = await service.QueryAsync(new DataQueryRequest("Show purchase orders"));

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Summary.Should().Be("Found 2 purchase orders with line items.");
        result.Tables.Should().HaveCount(2);

        // Master table
        result.Tables[0].TableName.Should().Be("Purchase Orders");
        result.Tables[0].Rows.Should().HaveCount(1);
        result.Tables[0].Rows[0]["poNumber"]?.ToString().Should().Be("PO-9921");

        // Sub-table
        result.Tables[1].TableName.Should().Contain("Lines");
        result.Tables[1].Rows.Should().HaveCount(1);
        result.Tables[1].Rows[0]["sku"]?.ToString().Should().Be("RAW-401");
    }

    [Fact]
    public async Task QueryAsync_WhenAgentThrows_ReturnsErrorResponse()
    {
        // Arrange
        var mockChatClient = new Mock<IChatClient>();

        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("API key expired"));

        var agent = new ChatClientAgent(mockChatClient.Object, name: "TestAgent");
        var service = new DataQueryService(agent, NullLogger<DataQueryService>.Instance);

        // Act
        var result = await service.QueryAsync(new DataQueryRequest("Show orders"));

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("API key expired");
        result.Tables.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_WhenAgentReturnsStringifiedDataJson_NormalizesSuccessfully()
    {
        // Arrange
        var mockChatClient = new Mock<IChatClient>();

        var sampleAgentJson = """
            {
              "summary": "Found 1 invoice.",
              "primaryEntityName": "Invoices",
              "success": true,
              "data": "[{\"invoiceId\":\"INV-101\",\"amount\":450.00,\"status\":\"paid\"}]"
            }
            """;

        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, sampleAgentJson)));

        var agent = new ChatClientAgent(mockChatClient.Object, name: "TestAgent");
        var service = new DataQueryService(agent, NullLogger<DataQueryService>.Instance);

        // Act
        var result = await service.QueryAsync(new DataQueryRequest("Show invoices"));

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Tables.Should().HaveCount(1);
        result.Tables[0].TableName.Should().Be("Invoices");
        result.Tables[0].Rows[0]["invoiceId"]?.ToString().Should().Be("INV-101");
    }
}
