using System.Text.Json;
using ChatWithYourData.ChatService.API.Models;
using Microsoft.Agents.AI;

namespace ChatWithYourData.ChatService.API.Services;

public interface IDataQueryService
{
    Task<DynamicDataQueryResponse> QueryAsync(DataQueryRequest request, CancellationToken cancellationToken = default);
}

public sealed class DataQueryService(
    AIAgent agent,
    ILogger<DataQueryService> logger) : IDataQueryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<DynamicDataQueryResponse> QueryAsync(DataQueryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $$"""
                User Request: {{request.Intent}}

                Instructions:
                1. Query the ERP data using the appropriate tool.
                2. Return your response as a valid JSON object matching this structure:
                {
                  "summary": "Brief summary of results",
                  "primaryEntityName": "Primary entity name (e.g. Invoices, Purchase Orders, Products)",
                  "data": "[<stringified JSON array of records returned by the tool>]",
                  "success": true
                }
                3. Output ONLY the JSON object.
                """;

            var result = await agent.RunAsync(prompt, null, null, cancellationToken: cancellationToken);
            var text = result.Text?.Trim() ?? string.Empty;

            if (text.StartsWith("```"))
            {
                var firstLine = text.IndexOf('\n');
                var lastLine = text.LastIndexOf("```");
                if (firstLine > 0 && lastLine > firstLine)
                {
                    text = text.Substring(firstLine + 1, lastLine - firstLine - 1).Trim();
                }
            }

            AgentStructuredOutput? response = null;
            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    response = JsonSerializer.Deserialize<AgentStructuredOutput>(text, JsonOptions);
                }
                catch { }
            }

            if (response != null && !string.IsNullOrWhiteSpace(response.Data))
            {
                using var doc = JsonDocument.Parse(response.Data);
                var tables = JsonTableNormalizer.Normalize(doc.RootElement, response.PrimaryEntityName);

                return new DynamicDataQueryResponse(
                    Success: response.Success,
                    Summary: response.Summary,
                    Tables: tables,
                    RawJson: response.Data
                );
            }

            // Fallback: if model returned direct array/object without wrapper
            if (!string.IsNullOrWhiteSpace(text) && (text.StartsWith('[') || text.StartsWith('{')))
            {
                try
                {
                    using var directDoc = JsonDocument.Parse(text);
                    var tables = JsonTableNormalizer.Normalize(directDoc.RootElement);
                    return new DynamicDataQueryResponse(
                        Success: true,
                        Summary: "Query completed.",
                        Tables: tables,
                        RawJson: text
                    );
                }
                catch { }
            }

            return new DynamicDataQueryResponse(
                Success: true,
                Summary: "No records found matching your query.",
                Tables: []
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ERP data query for intent '{Intent}'", request.Intent);
            return new DynamicDataQueryResponse(
                Success: false,
                Summary: "An error occurred while querying the ERP agent.",
                Tables: [],
                ErrorMessage: ex.Message
            );
        }
    }
}
