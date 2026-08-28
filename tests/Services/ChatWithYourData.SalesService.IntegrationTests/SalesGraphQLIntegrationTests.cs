using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChatWithYourData.SalesService.IntegrationTests;

public class SalesGraphQLIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SalesGraphQLIntegrationTests(WebApplicationFactory<Program> factory)
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
        content.Should().Contain("SalesService GraphQL is running");
    }

    [Fact]
    public async Task SalesOrdersQuery_ReturnsSeededOrdersWithCustomerAndLines()
    {
        // Arrange
        var requestBody = new
        {
            query = @"
            {
              salesOrders {
                nodes {
                  id
                  orderNumber
                  totalAmount
                  status
                  customer {
                    name
                    email
                  }
                  lines {
                    sku
                    productName
                    quantity
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
        jsonResponse.Should().Contain("SO-00001");
        jsonResponse.Should().Contain("Acme Technologies Corp");
        jsonResponse.Should().Contain("PRD-LAP-001");
    }

    [Fact]
    public async Task SalesOrderLinesQuery_ReturnsProductStub()
    {
        // Arrange
        var requestBody = new
        {
            query = @"
            {
              salesOrders {
                nodes {
                  orderNumber
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
        jsonResponse.Should().Contain("SO-00001");
        jsonResponse.Should().Contain("product");
    }
}
