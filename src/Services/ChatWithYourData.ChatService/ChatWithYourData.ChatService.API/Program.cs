using System.Runtime.CompilerServices;
using System.Text.Json;
using ChatWithYourData.ChatService.API.Configuration;
using ChatWithYourData.ChatService.API.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Configure port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5005); // ChatService / Agent on port 5005
});

// Configure Options
builder.Services.Configure<AgentOptions>(
    builder.Configuration.GetSection(AgentOptions.SectionName));

var agentOptions = builder.Configuration
    .GetSection(AgentOptions.SectionName)
    .Get<AgentOptions>() ?? new AgentOptions();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register MCP Tool Provider
builder.Services.AddSingleton<IMcpToolProvider, McpToolProvider>();

// Register IChatClient
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AgentOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(options.ApiKey))
    {
        var clientOptions = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(options.Endpoint))
        {
            clientOptions.Endpoint = new Uri(options.Endpoint);
        }

        var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(options.ApiKey), clientOptions);
        return openAiClient
            .GetChatClient(options.Model)
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();
    }

    // Offline / Mock IChatClient for development & testing
    return new MockErpChatClient();
});

// Register AIAgent
builder.Services.AddSingleton<AIAgent>(sp =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    var toolProvider = sp.GetRequiredService<IMcpToolProvider>();
    var tools = toolProvider.GetToolsAsync().GetAwaiter().GetResult();

    var agent = new ChatClientAgent(
        chatClient: chatClient,
        name: agentOptions.Name,
        instructions: agentOptions.Instructions,
        tools: tools);

    return agent;
});

// Register AG-UI Server Support
builder.Services.AddAGUIServer();

var app = builder.Build();

app.UseCors("AllowAll");

// Map AG-UI Protocol Endpoint
var agent = app.Services.GetRequiredService<AIAgent>();
app.MapAGUIServer(agentOptions.AgUiEndpoint, agent);

// Map Diagnostics & Health Endpoints
app.MapGet("/", async (IMcpToolProvider toolProvider, Microsoft.Extensions.Options.IOptions<AgentOptions> options) =>
{
    var opt = options.Value;
    var isMcpHealthy = await toolProvider.IsHealthyAsync();
    var tools = await toolProvider.GetToolsAsync();

    return Results.Ok(new
    {
        Service = "ChatWithYourData.ChatService",
        Framework = "Microsoft.Agents.AI 1.19.0",
        Protocol = "AG-UI (SSE)",
        Status = "Healthy",
        Endpoints = new
        {
            AgUi = opt.AgUiEndpoint,
            Health = "/health"
        },
        Agent = new
        {
            Name = opt.Name,
            DisplayName = opt.DisplayName,
            Provider = opt.Provider,
            Model = opt.Model,
            Endpoint = opt.Endpoint,
            ToolsCount = tools.Count
        },
        McpGateway = new
        {
            Endpoint = opt.McpGatewayEndpoint,
            IsConnected = isMcpHealthy
        }
    });
});

app.MapGet("/health", async (IMcpToolProvider toolProvider) =>
{
    var mcpHealthy = await toolProvider.IsHealthyAsync();
    return Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        McpGatewayStatus = mcpHealthy ? "Connected" : "Degraded/Offline"
    });
});

app.Run();

/// <summary>
/// Offline / Mock implementation of IChatClient for testing and local exploration without API keys.
/// </summary>
public sealed class MockErpChatClient : IChatClient
{
    public ChatClientMetadata Metadata => new("MockErpChatClient", new Uri("http://localhost:5005"));

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var lastMessage = chatMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;
        var replyText = GenerateResponse(lastMessage);

        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, replyText))
        {
            ModelId = "mock-erp-gpt4o",
            FinishReason = ChatFinishReason.Stop
        };

        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastMessage = chatMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;
        var replyText = GenerateResponse(lastMessage);

        var words = replyText.Split(' ');
        var responseId = "msg-" + Guid.NewGuid().ToString("N");
        for (var i = 0; i < words.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chunk = (i == 0 ? "" : " ") + words[i];
            yield return new ChatResponseUpdate(ChatRole.Assistant, chunk)
            {
                ResponseId = responseId,
                ModelId = "mock-erp-gpt4o"
            };
            await Task.Delay(20, cancellationToken);
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType == typeof(IChatClient) ? this : null;

    public void Dispose() { }

    private static string GenerateResponse(string prompt)
    {
        var p = prompt.ToLowerInvariant();
        if (p.Contains("product") || p.Contains("inventory") || p.Contains("stock"))
        {
            return "I queried the **Inventory Service** via MCP tool `get_products`.\n\n" +
                   "| SKU | Product Name | Stock on Hand | Unit Price |\n" +
                   "| :--- | :--- | :--- | :--- |\n" +
                   "| `PROD-001` | Widget Alpha | 150 units | $19.99 |\n" +
                   "| `PROD-002` | Gadget Pro | 42 units | $149.50 |\n\n" +
                   "Stock levels are optimal across warehouses.";
        }

        if (p.Contains("sales") || p.Contains("order") || p.Contains("customer"))
        {
            return "I queried the **Sales Service** via MCP tool `get_sales_orders`.\n\n" +
                   "- **Sales Order #SO-1001**: Confirmed ($450.00) for Customer `CUST-101` (Acme Corp)\n" +
                   "- **Sales Order #SO-1002**: Shipped ($1,280.00) for Customer `CUST-102` (Global Tech)\n\n" +
                   "Fulfillment pipeline is moving on schedule.";
        }

        if (p.Contains("purchase") || p.Contains("vendor") || p.Contains("procurement"))
        {
            return "I queried the **Procurement Service** via MCP tool `get_purchase_orders`.\n\n" +
                   "- **Purchase Order #PO-9001**: Approved ($1,200.00) with Vendor `VEND-ACME`\n" +
                   "- **Purchase Order #PO-9002**: Received ($3,400.00) with Vendor `VEND-OMEGA`\n\n" +
                   "Replenishment orders are within approved budgets.";
        }

        if (p.Contains("finance") || p.Contains("invoice") || p.Contains("balance") || p.Contains("ledger"))
        {
            return "I queried the **Finance Service** via MCP tool `get_invoices`.\n\n" +
                   "- **Total Outstanding Invoices**: $1,730.00\n" +
                   "- **Recent Invoice #INV-2024-001**: Amount $450.00 (Status: Unpaid, Due in 30 days)\n" +
                   "- **General Ledger**: Balanced debit/credit entries.";
        }

        return $"Hello! I am your **ChatWithYourData ERP AI Assistant**. I can help you search and manage products, sales orders, purchase orders, and financial records across the federated ERP system. You said: \"{prompt}\"";
    }
}

namespace ChatWithYourData.ChatService.API
{
    public partial class Program { }
}
