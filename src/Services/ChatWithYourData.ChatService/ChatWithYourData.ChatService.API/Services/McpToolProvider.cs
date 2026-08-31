using System.Text.Json;
using ChatWithYourData.ChatService.API.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;

namespace ChatWithYourData.ChatService.API.Services;

/// <summary>
/// Implements MCP tool discovery and execution via the ChilliCream Fusion Gateway MCP endpoint.
/// </summary>
public sealed class McpToolProvider : IMcpToolProvider, IAsyncDisposable
{
    private readonly AgentOptions _options;
    private readonly ILogger<McpToolProvider> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private McpClient? _mcpClient;
    private IList<AITool>? _cachedLiveTools;

    public McpToolProvider(IOptions<AgentOptions> options, ILogger<McpToolProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedLiveTools != null)
        {
            return _cachedLiveTools;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedLiveTools != null)
            {
                return _cachedLiveTools;
            }

            try
            {
                var client = await GetOrCreateClientAsync(cancellationToken);
                var mcpTools = await client.ListToolsAsync(cancellationToken: cancellationToken);
                
                var aiTools = new List<AITool>();
                foreach (var tool in mcpTools)
                {
                    aiTools.Add(tool);
                    _logger.LogInformation("Discovered MCP Tool: {ToolName} - {Description}", tool.Name, tool.Description);
                }

                _cachedLiveTools = aiTools;
                return _cachedLiveTools;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not reach MCP Gateway at {Endpoint}. Providing fallback ERP tools until Gateway is reachable.", _options.McpGatewayEndpoint);
                _mcpClient = null;
                return GetFallbackTools();
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetOrCreateClientAsync(cancellationToken);
            var tools = await client.ListToolsAsync(cancellationToken: cancellationToken);
            if (tools != null && _cachedLiveTools == null)
            {
                var aiTools = new List<AITool>();
                foreach (var tool in tools)
                {
                    aiTools.Add(tool);
                }
                _cachedLiveTools = aiTools;
            }
            return tools != null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Health check failed for MCP Gateway at {Endpoint}", _options.McpGatewayEndpoint);
            _mcpClient = null;
            return false;
        }
    }

    private async Task<McpClient> GetOrCreateClientAsync(CancellationToken cancellationToken)
    {
        if (_mcpClient != null)
        {
            return _mcpClient;
        }

        _logger.LogInformation("Connecting to MCP Gateway at {Endpoint}", _options.McpGatewayEndpoint);

        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(_options.McpGatewayEndpoint),
            TransportMode = HttpTransportMode.StreamableHttp,
            ConnectionTimeout = TimeSpan.FromSeconds(5)
        });

        _mcpClient = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
        return _mcpClient;
    }

    private static IList<AITool> GetFallbackTools()
    {
        // Provide typed ERP functions in case the gateway is temporarily offline or in unit tests
        return
        [
            AIFunctionFactory.Create(
                ([System.ComponentModel.Description("Product name or SKU to search for")] string? query) =>
                    $"[ERP Mock] Products matching '{query}': [SKU: PROD-001 - Widget A, Price: $19.99, Stock: 150 units in Warehouse-East]",
                "get_products",
                "Queries the Inventory microservice for products and stock levels."),

            AIFunctionFactory.Create(
                ([System.ComponentModel.Description("Sales Order ID or Customer Number")] string orderOrCustomer) =>
                    $"[ERP Mock] Orders for '{orderOrCustomer}': [Order #SO-1001, Status: Confirmed, Total: $450.00, Lines: 2]",
                "get_sales_orders",
                "Queries the Sales microservice for sales orders and fulfillment status."),

            AIFunctionFactory.Create(
                ([System.ComponentModel.Description("Purchase Order Number or Vendor Code")] string poOrVendor) =>
                    $"[ERP Mock] Purchase Orders for '{poOrVendor}': [PO #PO-9001, Vendor: VEND-ACME, Status: Approved, TotalCost: $1,200.00]",
                "get_purchase_orders",
                "Queries the Procurement microservice for purchase orders and vendor details."),

            AIFunctionFactory.Create(
                ([System.ComponentModel.Description("Invoice ID or Customer ID")] string invoiceOrCustomer) =>
                    $"[ERP Mock] Invoices for '{invoiceOrCustomer}': [Invoice #INV-2024-001, Amount: $450.00, Paid: $0.00, Status: Unpaid, Due: 30 days]",
                "get_invoices",
                "Queries the Finance microservice for invoices, balances, and payment status.")
        ];
    }

    public async ValueTask DisposeAsync()
    {
        if (_mcpClient != null)
        {
            await _mcpClient.DisposeAsync();
            _mcpClient = null;
        }
        _lock.Dispose();
    }
}
