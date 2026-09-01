using System.Text.Json;
using ChatWithYourData.ChatService.API.Services;
using FluentAssertions;

namespace ChatWithYourData.ChatService.UnitTests;

public class JsonTableNormalizerTests
{
    [Fact]
    public void Normalize_WhenFlatArray_ProducesSingleTableWithInferredColumns()
    {
        // Arrange
        var json = """
            [
              {
                "id": "ITEM-101",
                "name": "Silicon Wafers",
                "unitPrice": 125.50,
                "quantity": 40,
                "status": "in_stock",
                "createdAt": "2026-08-31"
              },
              {
                "id": "ITEM-102",
                "name": "Copper Clad Laminates",
                "unitPrice": 42.00,
                "quantity": 150,
                "status": "critical_low",
                "createdAt": "2026-08-30"
              }
            ]
            """;
        var doc = JsonDocument.Parse(json);

        // Act
        var tables = JsonTableNormalizer.Normalize(doc.RootElement, "Products");

        // Assert
        tables.Should().HaveCount(1);
        var table = tables[0];
        table.TableName.Should().Be("Products");
        table.Rows.Should().HaveCount(2);
        table.Columns.Should().Contain(c => c.Key == "unitPrice" && c.Type == "currency");
        table.Columns.Should().Contain(c => c.Key == "quantity" && c.Type == "number");
        table.Columns.Should().Contain(c => c.Key == "status" && c.Type == "badge");
        table.Columns.Should().Contain(c => c.Key == "createdAt" && c.Type == "date");
    }

    [Fact]
    public void Normalize_WhenNestedOneToOneObject_FlattensIntoParentTable()
    {
        // Arrange
        var json = """
            [
              {
                "poNumber": "PO-9921",
                "totalAmount": 5400.00,
                "vendor": {
                  "id": "V-101",
                  "companyName": "Kyocera Precision"
                }
              }
            ]
            """;
        var doc = JsonDocument.Parse(json);

        // Act
        var tables = JsonTableNormalizer.Normalize(doc.RootElement, "Purchase Orders");

        // Assert
        tables.Should().HaveCount(1);
        var table = tables[0];
        table.Rows.Should().HaveCount(1);
        table.Rows[0].Should().ContainKey("vendor_companyName");
        table.Rows[0]["vendor_companyName"]?.ToString().Should().Be("Kyocera Precision");
    }

    [Fact]
    public void Normalize_WhenNestedOneToManyArray_DecomposesIntoMasterAndSubTable()
    {
        // Arrange
        var json = """
            [
              {
                "poNumber": "PO-9921",
                "orderDate": "2026-08-30",
                "status": "pending_approval",
                "lines": [
                  { "sku": "RAW-401", "name": "Silicon Wafers", "quantity": 100, "unitPrice": 125.00 },
                  { "sku": "RAW-408", "name": "Copper Laminates", "quantity": 500, "unitPrice": 42.50 }
                ]
              },
              {
                "poNumber": "PO-9922",
                "orderDate": "2026-08-31",
                "status": "fulfilled",
                "lines": [
                  { "sku": "RAW-500", "name": "Heat Sinks", "quantity": 50, "unitPrice": 80.00 }
                ]
              }
            ]
            """;
        var doc = JsonDocument.Parse(json);

        // Act
        var tables = JsonTableNormalizer.Normalize(doc.RootElement, "Purchase Orders");

        // Assert
        tables.Should().HaveCount(2);

        // Master Table
        var master = tables[0];
        master.TableName.Should().Be("Purchase Orders");
        master.Rows.Should().HaveCount(2);

        // Sub Table
        var subTable = tables[1];
        subTable.TableName.Should().Contain("Lines");
        subTable.ParentKeyName.Should().Be("poNumber");
        subTable.Rows.Should().HaveCount(3); // 2 from first PO + 1 from second PO
        subTable.Rows.Should().Contain(r => r.ContainsKey("poNumber") && r["poNumber"]!.ToString() == "PO-9921");
        subTable.Rows.Should().Contain(r => r.ContainsKey("poNumber") && r["poNumber"]!.ToString() == "PO-9922");
    }
}
