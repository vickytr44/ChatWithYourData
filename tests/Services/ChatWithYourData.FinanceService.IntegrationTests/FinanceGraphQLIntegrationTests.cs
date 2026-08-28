using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChatWithYourData.FinanceService.IntegrationTests;

public class FinanceGraphQLIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FinanceGraphQLIntegrationTests(WebApplicationFactory<Program> factory)
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
        content.Should().Contain("FinanceService GraphQL is running");
    }

    [Fact]
    public async Task InvoicesQuery_ReturnsSeededInvoicesWithPayments()
    {
        // Arrange
        var requestBody = new
        {
            query = @"
            {
              invoices {
                nodes {
                  id
                  invoiceNumber
                  totalAmount
                  paidAmount
                  status
                  payments {
                    paymentNumber
                    amount
                    method
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
        jsonResponse.Should().Contain("INV-00001");
        jsonResponse.Should().Contain("PAY-00001");
    }

    [Fact]
    public async Task PaymentsQuery_ReturnsInvoiceWithCustomerStub()
    {
        // Arrange
        var requestBody = new
        {
            query = @"
            {
              payments {
                nodes {
                  paymentNumber
                  invoice {
                    invoiceNumber
                    customer {
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
        jsonResponse.Should().Contain("PAY-00001");
        jsonResponse.Should().Contain("customer");
    }
}
