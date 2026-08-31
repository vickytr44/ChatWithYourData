namespace ChatWithYourData.ChatService.API.Configuration;

/// <summary>
/// Configuration options for the AI Agent and AG-UI Server.
/// </summary>
public sealed class AgentOptions
{
    public const string SectionName = "Agent";

    /// <summary>
    /// Gets or sets the name identifier of the agent.
    /// </summary>
    public string Name { get; set; } = "ChatWithYourDataERP";

    /// <summary>
    /// Gets or sets the display name / title for AG-UI clients.
    /// </summary>
    public string DisplayName { get; set; } = "ERP Intelligent Assistant";

    /// <summary>
    /// Gets or sets the system instructions/prompt guiding the agent's behavior.
    /// </summary>
    public string Instructions { get; set; } = """
        You are the ChatWithYourData enterprise ERP AI Assistant.
        You have direct access to ERP tools across 4 core bounded contexts:
        1. Inventory & Products (products, stock levels, warehouse inventory, stock adjustments)
        2. Sales & Customers (customer lookup, sales orders, order status)
        3. Procurement & Vendors (vendor lookup, purchase orders, purchase requests)
        4. Finance & Invoicing (chart of accounts, ledger, invoices, payments, financial health)

        Always use the provided MCP tools to fetch live ERP data or perform actions.
        Format numbers, currency ($), dates, and line items clearly in Markdown tables and lists.
        """;

    /// <summary>
    /// Gets or sets the LLM provider to use (e.g., "GoogleGemini", "OpenAI", "AzureOpenAI", "Ollama", "Mock").
    /// </summary>
    public string Provider { get; set; } = "GoogleGemini";

    /// <summary>
    /// Gets or sets the API Key for the model provider.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the Model / Deployment Name (e.g., "gemini-2.5-flash", "gpt-4o").
    /// </summary>
    public string Model { get; set; } = "gemini-2.5-flash";

    /// <summary>
    /// Gets or sets the OpenAI-compatible custom endpoint URI (e.g., "https://generativelanguage.googleapis.com/v1beta/openai/").
    /// </summary>
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/openai/";

    /// <summary>
    /// Gets or sets the ChilliCream Fusion MCP Gateway endpoint.
    /// </summary>
    public string McpGatewayEndpoint { get; set; } = "http://localhost:5000/graphql/mcp";

    /// <summary>
    /// Gets or sets the route prefix for the AG-UI protocol endpoint.
    /// </summary>
    public string AgUiEndpoint { get; set; } = "/ag-ui";
}
