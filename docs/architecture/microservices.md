# ERP Microservices Architecture

## Overview
ChatWithYourData ERP backend is decomposed into **4 core business bounded contexts**. Each microservice adheres to Clean Architecture, maintains its own isolated SQLite database via EF Core 10, and exposes a GraphQL subgraph. The subgraphs are federated by the **ChilliCream Fusion Gateway**.

---

## 1. Inventory & Products Service (`ChatWithYourData.InventoryService`)
- **Role**: Manages the product catalog, warehouse locations, stock levels, and inventory movements.
- **Key Entities**:
  - `Products` (Id, Sku, Name, Description, UnitPrice, UnitOfMeasure, CategoryId, IsActive, CreatedAtUtc)
  - `Categories` (Id, Name, ParentCategoryId)
  - `Warehouses` (Id, Code, Name, LocationAddress)
  - `StockItems` (Id, ProductId, WarehouseId, QuantityOnHand, QuantityReserved, ReorderPoint)
  - `StockAdjustments` (Id, ProductId, WarehouseId, QuantityDelta, Reason, AdjustedAtUtc)
- **GraphQL Subgraph**: Exposes queries for products, stock levels by warehouse, low-stock alerts, and mutations for product management and stock adjustments.

---

## 2. Sales & Customers Service (`ChatWithYourData.SalesService`)
- **Role**: Manages customer master records, sales quotations, sales orders, and fulfillment.
- **Key Entities**:
  - `Customers` (Id, CustomerNumber, Name, Email, Phone, BillingAddress, CreditLimit, IsActive)
  - `SalesOrders` (Id, OrderNumber, CustomerId, OrderDateUtc, Status [Draft/Confirmed/Shipped/Completed/Cancelled], TotalAmount)
  - `SalesOrderLines` (Id, SalesOrderId, ProductId, Sku, Quantity, UnitPrice, DiscountPercentage, LineTotal)
  - `Shipments` (Id, SalesOrderId, TrackingNumber, ShippedAtUtc, Status)
- **GraphQL Subgraph**: Exposes customer lookups, sales orders pipeline, customer order history, and order creation/status update mutations.

---

## 3. Procurement & Vendors Service (`ChatWithYourData.ProcurementService`)
- **Role**: Manages vendor/supplier relationships, purchase orders, and goods receipts.
- **Key Entities**:
  - `Vendors` (Id, VendorCode, Name, ContactEmail, PaymentTermsDays, TaxId, IsActive)
  - `PurchaseOrders` (Id, PoNumber, VendorId, OrderDateUtc, ExpectedDeliveryDateUtc, Status [Draft/Approved/Ordered/Received/Closed], TotalCost)
  - `PurchaseOrderLines` (Id, PurchaseOrderId, ProductId, Sku, QuantityOrdered, UnitCost, LineTotal)
  - `GoodsReceipts` (Id, PurchaseOrderId, ReceivedDateUtc, ReceivedBy, Notes)
- **GraphQL Subgraph**: Exposes vendor listings, purchase order tracking, replenishment requests, and PO authorization mutations.

---

## 4. Finance & Invoicing Service (`ChatWithYourData.FinanceService`)
- **Role**: General ledger, accounts payable/receivable, invoice generation, payment processing, and chart of accounts.
- **Key Entities**:
  - `Accounts` (Id, AccountCode, Name, AccountType [Asset/Liability/Equity/Revenue/Expense], Balance)
  - `JournalEntries` (Id, EntryNumber, EntryDateUtc, Description, PostedAtUtc)
  - `JournalLines` (Id, JournalEntryId, AccountId, DebitAmount, CreditAmount, Memo)
  - `Invoices` (Id, InvoiceNumber, OrderId, CustomerId, IssueDateUtc, DueDateUtc, TotalAmount, PaidAmount, Status [Unpaid/PartiallyPaid/Paid/Overdue])
  - `Payments` (Id, InvoiceId, PaymentDateUtc, Amount, PaymentMethod, ReferenceNumber)
- **GraphQL Subgraph**: Exposes financial health queries, accounts ledger, outstanding invoices, profit/loss calculations, and journal posting mutations.

---

## Database Per Service Isolation
- Each microservice operates with its own independent `DbContext` and SQLite database file:
  - `inventory.db`
  - `sales.db`
  - `procurement.db`
  - `finance.db`
- In automated test suites, each service runs against in-memory SQLite (`DataSource=:memory:;Mode=Memory;Cache=Shared`).
