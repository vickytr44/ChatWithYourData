using ChatWithYourData.ChatService.API.Configuration;
using ChatWithYourData.ChatService.API.Models;
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
    if (string.IsNullOrWhiteSpace(options.ApiKey))
    {
        throw new InvalidOperationException("API key is not configured for Agent. Please configure 'Agent:ApiKey' or set the environment variable.");
    }

    var clientOptions = new OpenAIClientOptions();
    if (!string.IsNullOrWhiteSpace(options.Endpoint))
    {
        clientOptions.Endpoint = new Uri(options.Endpoint);
    }

    // Attach policy to preserve Google Gemini thought_signature during multi-turn function calls
    clientOptions.AddPolicy(new GeminiThoughtSignaturePolicy(), System.ClientModel.Primitives.PipelinePosition.PerCall);

    var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(options.ApiKey), clientOptions);
    return openAiClient
        .GetChatClient(options.Model)
        .AsIChatClient()
        .AsBuilder()
        .UseFunctionInvocation()
        .Build();
});

// Register AIAgent
builder.Services.AddSingleton<AIAgent>(sp =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    var toolProvider = sp.GetRequiredService<IMcpToolProvider>();
    
    IList<AITool> tools;
    try
    {
        tools = toolProvider.GetToolsAsync().GetAwaiter().GetResult();
    }
    catch
    {
        tools = [];
    }

    var agent = new ChatClientAgent(
        chatClient: chatClient,
        name: agentOptions.Name,
        instructions: agentOptions.Instructions,
        tools: tools);

    return agent;
});

// Register AG-UI Server Support
builder.Services.AddAGUIServer();

// Register Data Query Service for structured MCP query data endpoint
builder.Services.AddSingleton<IDataQueryService, DataQueryService>();

var app = builder.Build();

app.UseCors("AllowAll");

// Map AG-UI Protocol Endpoint
var agent = app.Services.GetRequiredService<AIAgent>();
app.MapAGUIServer(agentOptions.AgUiEndpoint, agent);

// Map Structured Agent Data Query Endpoint
app.MapPost("/api/data/query", async (DataQueryRequest request, IDataQueryService queryService, CancellationToken ct) =>
{
    var response = await queryService.QueryAsync(request, ct);
    return Results.Ok(response);
});

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

namespace ChatWithYourData.ChatService.API
{
    public partial class Program { }
}
