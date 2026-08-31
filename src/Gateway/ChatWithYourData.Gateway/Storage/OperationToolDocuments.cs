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
              totalAmount
              notes
              customer {
                id
                customerNumber
                name
                email
                phone
                billingAddress
                shippingAddress
              }
              lines {
                id
                productId
                sku
                productName
                quantity
                unitPrice
                discountPercentage
                lineTotal
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
              unitPrice
              unitOfMeasure
              isActive
              category {
                id
                name
                description
              }
              stockItems {
                id
                warehouseId
                quantityOnHand
                quantityReserved
                reorderPoint
                warehouse {
                  id
                  code
                  name
                  locationAddress
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
              customerId
              salesOrderId
              issueDateUtc
              dueDateUtc
              status
              subtotal
              taxAmount
              totalAmount
              paidAmount
              notes
              payments {
                id
                paymentNumber
                paymentDateUtc
                amount
                method
                referenceNumber
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
              notes
              vendor {
                id
                vendorCode
                name
                contactEmail
                phone
                address
                paymentTermsDays
                taxId
              }
              lines {
                id
                productId
                sku
                productName
                quantityOrdered
                quantityReceived
                unitCost
                lineTotal
              }
              goodsReceipts {
                id
                receiptNumber
                receivedDateUtc
                receivedBy
                notes
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
              accountCode
              name
              type
              description
              currentBalance
              isActive
            }
          }
          journalEntries(where: $whereJournals, first: $first) {
            nodes {
              id
              entryNumber
              entryDateUtc
              description
              reference
              isPosted
              lines {
                id
                accountId
                debitAmount
                creditAmount
                memo
                account {
                  id
                  accountCode
                  name
                }
              }
            }
          }
        }
        """;
}
