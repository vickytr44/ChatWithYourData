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
                new(Utf8GraphQLParser.Parse("""
                    query SearchSalesOrders(
                      $where: SalesOrderFilterInput
                      $order: [SalesOrderSortInput!]
                      $first: Int
                    ) {
                      salesOrders(where: $where, order: $order, first: $first) {
                        nodes {
                          id
                          orderNumber
                          orderDateUtc
                          status
                          subtotal
                          taxAmount
                          totalAmount
                          customer {
                            id
                            customerNumber
                            name
                            email
                            phone
                            billingAddress
                          }
                          lines {
                            id
                            quantity
                            unitPrice
                            subtotal
                            product {
                              id
                              sku
                              name
                              unitPrice
                            }
                          }
                        }
                      }
                    }
                """))
                {
                    Name = "search_sales_orders"
                },
                new(Utf8GraphQLParser.Parse("""
                    query SearchProductsAndInventory(
                      $where: ProductFilterInput
                      $order: [ProductSortInput!]
                      $first: Int
                    ) {
                      products(where: $where, order: $order, first: $first) {
                        nodes {
                          id
                          sku
                          name
                          description
                          category
                          unitPrice
                          reorderPoint
                          isActive
                          stockItems {
                            id
                            warehouseId
                            quantityOnHand
                            allocatedQuantity
                            availableQuantity
                            warehouse {
                              id
                              name
                              location
                            }
                          }
                        }
                      }
                    }
                """))
                {
                    Name = "search_products_and_inventory"
                },
                new(Utf8GraphQLParser.Parse("""
                    query SearchInvoicesAndPayments(
                      $where: InvoiceFilterInput
                      $order: [InvoiceSortInput!]
                      $first: Int
                    ) {
                      invoices(where: $where, order: $order, first: $first) {
                        nodes {
                          id
                          invoiceNumber
                          issueDateUtc
                          dueDateUtc
                          status
                          subtotal
                          taxAmount
                          totalAmount
                          paidAmount
                          customer {
                            id
                            customerNumber
                            name
                            email
                          }
                          payments {
                            id
                            paymentNumber
                            amount
                            paymentDateUtc
                            paymentMethod
                            reference
                          }
                        }
                      }
                    }
                """))
                {
                    Name = "search_invoices_and_payments"
                },
                new(Utf8GraphQLParser.Parse("""
                    query SearchPurchaseOrders(
                      $where: PurchaseOrderFilterInput
                      $order: [PurchaseOrderSortInput!]
                      $first: Int
                    ) {
                      purchaseOrders(where: $where, order: $order, first: $first) {
                        nodes {
                          id
                          poNumber
                          orderDateUtc
                          expectedDeliveryDateUtc
                          status
                          totalCost
                          vendor {
                            id
                            vendorNumber
                            name
                            email
                          }
                          lines {
                            id
                            quantity
                            unitCost
                            totalCost
                            product {
                              id
                              sku
                              name
                            }
                          }
                          receipts {
                            id
                            receiptNumber
                            receiptDateUtc
                            status
                          }
                        }
                      }
                    }
                """))
                {
                    Name = "search_purchase_orders"
                },
                new(Utf8GraphQLParser.Parse("""
                    query SearchFinancialGL(
                      $whereAccounts: AccountFilterInput
                      $whereJournals: JournalEntryFilterInput
                      $first: Int
                    ) {
                      accounts(where: $whereAccounts, first: $first) {
                        nodes {
                          id
                          code
                          name
                          type
                          balance
                          currency
                          isActive
                        }
                      }
                      journalEntries(where: $whereJournals, first: $first) {
                        nodes {
                          id
                          entryNumber
                          entryDateUtc
                          memo
                          isPosted
                          lines {
                            id
                            accountId
                            debit
                            credit
                            memo
                            account {
                              code
                              name
                            }
                          }
                        }
                      }
                    }
                """))
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
