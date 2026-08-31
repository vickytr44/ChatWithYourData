using HotChocolate.Adapters.Mcp.Storage;
using HotChocolate.Language;

namespace ChatWithYourData.Gateway.Storage;

public class InMemoryMcpStorage : IMcpStorage
{
    private readonly List<OperationToolDefinition> _tools = new();
    private readonly List<PromptDefinition> _prompts = new();

    public InMemoryMcpStorage(
        IEnumerable<OperationToolDefinition>? tools = null,
        IEnumerable<PromptDefinition>? prompts = null)
    {
        if (tools != null)
        {
            _tools.AddRange(tools);
        }
        else
        {
            _tools.AddRange(new OperationToolDefinition[]
            {
                new(Utf8GraphQLParser.Parse(OperationToolDocuments.SearchSalesOrders))
                {
                    Name = "search_sales_orders"
                },
                new(Utf8GraphQLParser.Parse(OperationToolDocuments.SearchProductsAndInventory))
                {
                    Name = "search_products_and_inventory"
                },
                new(Utf8GraphQLParser.Parse(OperationToolDocuments.SearchInvoicesAndPayments))
                {
                    Name = "search_invoices_and_payments"
                },
                new(Utf8GraphQLParser.Parse(OperationToolDocuments.SearchPurchaseOrders))
                {
                    Name = "search_purchase_orders"
                },
                new(Utf8GraphQLParser.Parse(OperationToolDocuments.SearchFinancialGL))
                {
                    Name = "search_financial_gl"
                }
            });
        }

        if (prompts != null)
        {
            _prompts.AddRange(prompts);
        }
    }

    public ValueTask<IEnumerable<OperationToolDefinition>> GetOperationToolDefinitionsAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult<IEnumerable<OperationToolDefinition>>(_tools);

    public ValueTask<IEnumerable<PromptDefinition>> GetPromptDefinitionsAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult<IEnumerable<PromptDefinition>>(_prompts);

    public IDisposable Subscribe(IObserver<OperationToolStorageEventArgs> observer) => EmptyDisposable.Instance;
    public IDisposable Subscribe(IObserver<PromptStorageEventArgs> observer) => EmptyDisposable.Instance;

    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly EmptyDisposable Instance = new();
        public void Dispose() { }
    }
}
