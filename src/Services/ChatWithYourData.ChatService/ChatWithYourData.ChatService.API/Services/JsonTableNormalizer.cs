using System.Text.Json;
using System.Text.RegularExpressions;
using ChatWithYourData.ChatService.API.Models;

namespace ChatWithYourData.ChatService.API.Services;

/// <summary>
/// High-performance deterministic engine to normalize arbitrary nested JSON data into relational tables.
/// </summary>
public static class JsonTableNormalizer
{
    public static List<TableData> Normalize(JsonElement root, string? preferredRootName = null)
    {
        var tables = new List<TableData>();

        // 1. Resolve Root Array
        var (rootArray, inferredRootName) = ExtractArray(root);
        var rootTableName = !string.IsNullOrWhiteSpace(preferredRootName) ? preferredRootName : inferredRootName ?? "Primary Records";

        if (rootArray.Count == 0)
        {
            return [new TableData(rootTableName, "No records returned", null, [], [])];
        }

        // 2. Identify Primary Key for Linking
        var primaryKeyName = FindPrimaryKey(rootArray[0]);

        // 3. Process Master Table & Collect Nested Arrays
        var masterRows = new List<Dictionary<string, object?>>();
        var childCollections = new Dictionary<string, List<(object ParentIdValue, JsonElement ChildElement)>>();

        for (int i = 0; i < rootArray.Count; i++)
        {
            var item = rootArray[i];
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            var row = new Dictionary<string, object?>();
            object parentId = i + 1; // Default row index

            // Extract primary key value if available
            if (primaryKeyName != null && item.TryGetProperty(primaryKeyName, out var pkProp))
            {
                parentId = GetScalarValue(pkProp) ?? i + 1;
            }

            foreach (var prop in item.EnumerateObject())
            {
                var propName = prop.Name;
                var propValue = prop.Value;

                switch (propValue.ValueKind)
                {
                    case JsonValueKind.Object:
                        // 1-to-1 Object: Flatten into current row
                        FlattenObject(row, propName, propValue);
                        break;

                    case JsonValueKind.Array:
                        // 1-to-Many Array: Collect for Child Sub-Table
                        if (!childCollections.ContainsKey(propName))
                        {
                            childCollections[propName] = [];
                        }

                        foreach (var childItem in propValue.EnumerateArray())
                        {
                            childCollections[propName].Add((parentId, childItem));
                        }
                        break;

                    default:
                        // Scalar Value
                        row[propName] = GetScalarValue(propValue);
                        break;
                }
            }

            masterRows.Add(row);
        }

        var masterColumns = InferColumns(masterRows);
        tables.Add(new TableData(rootTableName, $"Primary {rootTableName} records", null, masterColumns, masterRows));

        // 4. Process Child Sub-Tables
        var parentLinkKey = primaryKeyName ?? "_parentId";
        foreach (var (childPropName, childItems) in childCollections)
        {
            if (childItems.Count == 0) continue;

            var childRows = new List<Dictionary<string, object?>>();
            var childTableName = FormatTitle(childPropName);

            foreach (var (pId, childElement) in childItems)
            {
                var childRow = new Dictionary<string, object?>
                {
                    [parentLinkKey] = pId
                };

                if (childElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var childProp in childElement.EnumerateObject())
                    {
                        if (childProp.Value.ValueKind == JsonValueKind.Object)
                        {
                            FlattenObject(childRow, childProp.Name, childProp.Value);
                        }
                        else if (childProp.Value.ValueKind != JsonValueKind.Array)
                        {
                            childRow[childProp.Name] = GetScalarValue(childProp.Value);
                        }
                    }
                }
                else
                {
                    childRow["value"] = GetScalarValue(childElement);
                }

                childRows.Add(childRow);
            }

            var childColumns = InferColumns(childRows, priorityKey: parentLinkKey);
            tables.Add(new TableData(childTableName, $"Sub-table for {rootTableName} -> {childTableName}", parentLinkKey, childColumns, childRows));
        }

        return tables;
    }

    private static (List<JsonElement> Elements, string? InferredName) ExtractArray(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            var list = new List<JsonElement>();
            foreach (var item in root.EnumerateArray())
            {
                list.Add(item);
            }
            return (list, null);
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            // Check if object has a single array property or a GraphQL "data" wrapper
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<JsonElement>();
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        list.Add(item);
                    }
                    return (list, FormatTitle(prop.Name));
                }
                if (prop.Name.Equals("data", StringComparison.OrdinalIgnoreCase) && prop.Value.ValueKind == JsonValueKind.Object)
                {
                    return ExtractArray(prop.Value);
                }
            }

            // Single object -> wrap into 1-item array
            return ([root], null);
        }

        return ([], null);
    }

    private static void FlattenObject(Dictionary<string, object?> row, string prefix, JsonElement obj)
    {
        foreach (var prop in obj.EnumerateObject())
        {
            var key = $"{prefix}_{prop.Name}";
            if (prop.Value.ValueKind == JsonValueKind.Object)
            {
                FlattenObject(row, key, prop.Value);
            }
            else if (prop.Value.ValueKind != JsonValueKind.Array)
            {
                row[key] = GetScalarValue(prop.Value);
            }
        }
    }

    private static object? GetScalarValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private static string? FindPrimaryKey(JsonElement firstItem)
    {
        if (firstItem.ValueKind != JsonValueKind.Object)
            return null;

        var candidateNames = new[] { "id", "sku", "poNumber", "orderNumber", "invoiceNumber", "code", "key" };
        foreach (var name in candidateNames)
        {
            foreach (var prop in firstItem.EnumerateObject())
            {
                if (prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return prop.Name;
                }
            }
        }

        return null;
    }

    private static List<TableColumn> InferColumns(List<Dictionary<string, object?>> rows, string? priorityKey = null)
    {
        var keys = new HashSet<string>();
        foreach (var row in rows)
        {
            foreach (var key in row.Keys)
            {
                keys.Add(key);
            }
        }

        var columns = new List<TableColumn>();

        // Ensure priority/link key is first
        if (priorityKey != null && keys.Contains(priorityKey))
        {
            columns.Add(new TableColumn(priorityKey, FormatTitle(priorityKey), "string"));
            keys.Remove(priorityKey);
        }

        foreach (var key in keys)
        {
            var type = InferColumnType(key, rows);
            var label = FormatTitle(key);
            columns.Add(new TableColumn(key, label, type));
        }

        return columns;
    }

    private static string InferColumnType(string key, List<Dictionary<string, object?>> rows)
    {
        var lowerKey = key.ToLowerInvariant();

        if (lowerKey.Contains("price") || lowerKey.Contains("amount") || lowerKey.Contains("cost") || lowerKey.Contains("total") || lowerKey.Contains("tax") || lowerKey.Contains("balance"))
        {
            return "currency";
        }

        if (lowerKey.Contains("status") || lowerKey.Contains("state") || lowerKey.Contains("badge"))
        {
            return "badge";
        }

        if (lowerKey.Contains("date") || lowerKey.Contains("time") || lowerKey.Contains("at"))
        {
            return "date";
        }

        // Inspect actual values
        foreach (var row in rows)
        {
            if (row.TryGetValue(key, out var val) && val != null)
            {
                if (val is int or long or double or float or decimal)
                {
                    return "number";
                }
                if (val is string str && Regex.IsMatch(str, @"^\d{4}-\d{2}-\d{2}"))
                {
                    return "date";
                }
            }
        }

        return "string";
    }

    private static string FormatTitle(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return identifier;

        // Replace underscores with spaces
        var result = identifier.Replace("_", " ");

        // Insert space before uppercase letters in camelCase (e.g. unitPrice -> unit Price)
        result = Regex.Replace(result, @"([a-z])([A-Z])", "$1 $2");

        // Capitalize words
        var words = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..];
        }

        return string.Join(" ", words);
    }
}
