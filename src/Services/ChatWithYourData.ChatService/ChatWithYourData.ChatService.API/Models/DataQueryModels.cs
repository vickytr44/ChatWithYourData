namespace ChatWithYourData.ChatService.API.Models;

/// <summary>
/// Natural language request from the client.
/// </summary>
public record DataQueryRequest(
    string Intent
);

/// <summary>
/// Structured output schema expected from the LLM Agent.
/// </summary>
public record AgentStructuredOutput(
    string Summary,
    string Data,
    string? PrimaryEntityName = null,
    bool Success = true
);

/// <summary>
/// Column metadata definition for dynamic table rendering.
/// </summary>
public record TableColumn(
    string Key,
    string Label,
    string Type // "string" | "number" | "currency" | "badge" | "date"
);

/// <summary>
/// Normalized relational table representation.
/// </summary>
public record TableData(
    string TableName,
    string? Description,
    string? ParentKeyName,
    List<TableColumn> Columns,
    List<Dictionary<string, object?>> Rows
);

/// <summary>
/// Complete response returned by the backend endpoint.
/// </summary>
public record DynamicDataQueryResponse(
    bool Success,
    string Summary,
    List<TableData> Tables,
    string? RawJson = null,
    string? ErrorMessage = null
);
