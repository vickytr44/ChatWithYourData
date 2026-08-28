using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChatWithYourData.ProcurementService.IntegrationTests;

public class ProcurementGraphQLIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProcurementGraphQLIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootEndpoint_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ProcurementService GraphQL is running");
    }

    [Fact]
    public async Task PurchaseOrdersQuery_ReturnsSeededPoWithVendorAndLines()
    {
        // Arrange
        var requestBody = new
        {
            query = @"
            {
              purchaseOrders {
                nodes {
                  id
                  poNumber
                  totalCost
                  status
                  vendor {
                    name
                    contactEmail
                  }
                  lines {
                    sku
                    productName
                    quantityOrdered
                    lineTotal
                  }
                }
              }
            }"
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        jsonResponse.Should().NotContain("\"errors\":");
        jsonResponse.Should().Contain("PO-00001");
        jsonResponse.Should().Contain("Silicon Microdevices Inc");
        jsonResponse.Should().Contain("PRD-LAP-001");
    }

    [Fact]
    public async Task PurchaseOrderLinesQuery_ReturnsProductStub()
    {
        // Arrange
        var requestBody = new
        {
            query = @"
            {
              purchaseOrders {
                nodes {
                  poNumber
                  lines {
                    sku
                    product {
                      id
                    }
                  }
                }
              }
            }"
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        jsonResponse.Should().NotContain("\"errors\":");
        jsonResponse.Should().Contain("PO-00001");
        jsonResponse.Should().Contain("product");
    }
}
