using ChatWithYourData.Gateway.Storage;
using HotChocolate.Language;

var builder = WebApplication.CreateBuilder(args);

// Configure port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // Fusion Gateway on port 5000
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Configure Subgraph HTTP Clients
var fusionSection = builder.Configuration.GetSection("Fusion:Subgraphs");
var inventoryUri = fusionSection.GetValue<string>("Inventory") ?? "http://localhost:5001/graphql";
var salesUri = fusionSection.GetValue<string>("Sales") ?? "http://localhost:5002/graphql";
var procurementUri = fusionSection.GetValue<string>("Procurement") ?? "http://localhost:5003/graphql";
var financeUri = fusionSection.GetValue<string>("Finance") ?? "http://localhost:5004/graphql";

builder.Services.AddHttpClient("Inventory", client => client.BaseAddress = new Uri(inventoryUri));
builder.Services.AddHttpClient("Sales", client => client.BaseAddress = new Uri(salesUri));
builder.Services.AddHttpClient("Procurement", client => client.BaseAddress = new Uri(procurementUri));
builder.Services.AddHttpClient("Finance", client => client.BaseAddress = new Uri(financeUri));

// Configure Fusion Gateway and MCP Adapter
var gatewayBuilder = builder
    .AddGraphQLGateway()
    .AddMcp()
    .AddMcpStorage<InMemoryMcpStorage>();

var gatewayConfigFile = System.IO.Path.Combine(builder.Environment.ContentRootPath, "gateway.far");
if (System.IO.File.Exists(gatewayConfigFile))
{
    gatewayBuilder.AddFileSystemConfiguration(gatewayConfigFile);
}
else
{
    var fallbackDoc = Utf8GraphQLParser.Parse("""
        type Query {
            gatewayStatus: String!
        }

        enum fusion__Schema {
            Inventory
            Sales
            Procurement
            Finance
        }
    """);
    gatewayBuilder.AddInMemoryConfiguration(fallbackDoc, null!);
}

var app = builder.Build();

app.UseCors("AllowAll");

app.MapGraphQL();
app.MapGraphQLMcp();

app.MapGet("/", () => new
{
    Service = "ChatWithYourData.Gateway",
    Status = "Healthy",
    Endpoints = new
    {
        GraphQL = "/graphql",
        MCP = "/graphql/mcp"
    },
    Subgraphs = new
    {
        Inventory = inventoryUri,
        Sales = salesUri,
        Procurement = procurementUri,
        Finance = financeUri
    }
});

app.Run();

namespace ChatWithYourData.Gateway
{
    public partial class Program { }
}
