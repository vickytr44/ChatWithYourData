using HotChocolate.Adapters.Mcp.Storage;

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
