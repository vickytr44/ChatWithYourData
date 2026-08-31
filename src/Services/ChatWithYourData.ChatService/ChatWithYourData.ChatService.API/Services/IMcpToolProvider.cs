using Microsoft.Extensions.AI;

namespace ChatWithYourData.ChatService.API.Services;

/// <summary>
/// Service responsible for discovering and managing MCP tools from the Fusion Gateway.
/// </summary>
public interface IMcpToolProvider
{
    /// <summary>
    /// Connects to the MCP Gateway and retrieves the available ERP tools as AIFunctions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of AITools / AIFunctions ready for the AI Agent.</returns>
    Task<IList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the health/connectivity of the MCP Gateway.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the Gateway MCP server is reachable.</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
