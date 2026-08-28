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
                new(Utf8GraphQLParser.Parse("{ products { nodes { id sku name unitPrice } } }"))
                {
                    Name = "get_products"
                },
                new(Utf8GraphQLParser.Parse("{ salesOrders { nodes { id orderNumber status totalAmount } } }"))
                {
                    Name = "get_sales_orders"
                },
                new(Utf8GraphQLParser.Parse("{ purchaseOrders { nodes { id poNumber status totalCost } } }"))
                {
                    Name = "get_purchase_orders"
                },
                new(Utf8GraphQLParser.Parse("{ invoices { nodes { id invoiceNumber status totalAmount paidAmount } } }"))
                {
                    Name = "get_invoices"
                },
                new(Utf8GraphQLParser.Parse("mutation ($input: AdjustStockInput!) { adjustStock(input: $input) { data { id quantityOnHand } success error } }"))
                {
                    Name = "adjust_stock"
                },
                new(Utf8GraphQLParser.Parse("mutation ($input: CreateSalesOrderInput!) { createSalesOrder(input: $input) { data { id orderNumber } success error } }"))
                {
                    Name = "create_sales_order"
                },
                new(Utf8GraphQLParser.Parse("mutation ($input: CreatePurchaseOrderInput!) { createPurchaseOrder(input: $input) { data { id poNumber } success error } }"))
                {
                    Name = "create_purchase_order"
                },
                new(Utf8GraphQLParser.Parse("mutation ($input: PostJournalEntryInput!) { postJournalEntry(input: $input) { data { id entryNumber } success error } }"))
                {
                    Name = "post_journal_entry"
                }
            });
        }

        if (prompts != null)
        {
            _prompts.AddRange(prompts);
        }
    }

    public ValueTask<IEnumerable<OperationToolDefinition>> GetOperationToolDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<IEnumerable<OperationToolDefinition>>(_tools);
    }

    public ValueTask<IEnumerable<PromptDefinition>> GetPromptDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<IEnumerable<PromptDefinition>>(_prompts);
    }

    public IDisposable Subscribe(IObserver<OperationToolStorageEventArgs> observer)
    {
        return EmptyDisposable.Instance;
    }

    public IDisposable Subscribe(IObserver<PromptStorageEventArgs> observer)
    {
        return EmptyDisposable.Instance;
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly EmptyDisposable Instance = new();
        public void Dispose() { }
    }
}
