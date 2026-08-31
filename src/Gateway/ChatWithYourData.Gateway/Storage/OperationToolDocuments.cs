namespace ChatWithYourData.Gateway.Storage;

public static class OperationToolDocuments
{
    public const string SearchSalesOrders = """
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
        """;

    public const string SearchProductsAndInventory = """
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
        """;

    public const string SearchInvoicesAndPayments = """
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
        """;

    public const string SearchPurchaseOrders = """
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
        """;

    public const string SearchFinancialGL = """
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
        """;
}
