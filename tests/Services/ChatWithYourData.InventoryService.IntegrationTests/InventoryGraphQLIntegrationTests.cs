using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChatWithYourData.InventoryService.IntegrationTests;

public class InventoryGraphQLIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InventoryGraphQLIntegrationTests(WebApplicationFactory<Program> factory)
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
        content.Should().Contain("InventoryService GraphQL is running");
    }

    [Fact]
    public async Task ProductsQuery_ReturnsSeededProductsWithCategoriesAndStock()
    {
        // Arrange
        var requestBody = new
        {
            query = @"
            {
              products {
                nodes {
                  id
                  sku
                  name
                  unitPrice
                  category {
                    name
                  }
                  stockItems {
                    quantityOnHand
                    warehouse {
                      code
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
        jsonResponse.Should().Contain("PRD-LAP-001");
        jsonResponse.Should().Contain("Enterprise Pro Laptop 16\\\"");
    }
}
