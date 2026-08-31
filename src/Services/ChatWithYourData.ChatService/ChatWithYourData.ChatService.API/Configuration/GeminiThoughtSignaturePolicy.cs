using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace ChatWithYourData.ChatService.API.Configuration;

/// <summary>
/// Pipeline policy for Google Gemini models on the OpenAI-compatible endpoint.
/// Gemini 3.x thinking models require thought_signature from tool calls to be preserved
/// in the follow-up assistant message for summarization to succeed.
/// </summary>
public sealed class GeminiThoughtSignaturePolicy : PipelinePolicy
{
    private readonly ConcurrentDictionary<string, string> _thoughtSignatures = new();

    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ProcessAsync(message, pipeline, currentIndex).GetAwaiter().GetResult();
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        // 1. When sending request: restore thought_signature in tool_calls
        if (message.Request.Content != null)
        {
            using var ms = new MemoryStream();
            await message.Request.Content.WriteToAsync(ms, default);
            var jsonStr = Encoding.UTF8.GetString(ms.ToArray());

            if (jsonStr.Contains("tool_calls"))
            {
                try
                {
                    var node = JsonNode.Parse(jsonStr);
                    if (node?["messages"] is JsonArray messagesArr)
                    {
                        bool modified = false;
                        foreach (var msg in messagesArr)
                        {
                            if (msg?["tool_calls"] is JsonArray toolCalls)
                            {
                                foreach (var tc in toolCalls)
                                {
                                    var id = tc?["id"]?.GetValue<string>();
                                    if (id != null && _thoughtSignatures.TryGetValue(id, out var sig))
                                    {
                                        tc!["extra_content"] = new JsonObject
                                        {
                                            ["google"] = new JsonObject
                                            {
                                                ["thought_signature"] = sig
                                            }
                                        };
                                        modified = true;
                                    }
                                }
                            }
                        }

                        if (modified)
                        {
                            var newJson = node.ToJsonString();
                            message.Request.Content = BinaryContent.Create(BinaryData.FromString(newJson));
                        }
                    }
                }
                catch { }
            }
        }

        // 2. Call next policy in pipeline
        await ProcessNextAsync(message, pipeline, currentIndex);

        // 3. Wrap response ContentStream with ThoughtSignatureTrackingStream
        if (message.Response?.ContentStream != null)
        {
            message.Response.ContentStream = new ThoughtSignatureTrackingStream(
                message.Response.ContentStream,
                _thoughtSignatures);
        }
    }

    private sealed class ThoughtSignatureTrackingStream : Stream
    {
        private readonly Stream _inner;
        private readonly ConcurrentDictionary<string, string> _signatures;
        private readonly StringBuilder _buffer = new();

        public ThoughtSignatureTrackingStream(Stream inner, ConcurrentDictionary<string, string> signatures)
        {
            _inner = inner;
            _signatures = signatures;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => false; // Prevent ClientModel from asserting Position == 0 on dispose
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush() => _inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _inner.Read(buffer, offset, count);
            if (read > 0)
            {
                ProcessBytes(buffer, offset, read);
            }
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await _inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
            if (read > 0)
            {
                ProcessBytes(buffer, offset, read);
            }
            return read;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = await _inner.ReadAsync(buffer, cancellationToken);
            if (read > 0)
            {
                var bytes = buffer.Slice(0, read).ToArray();
                ProcessBytes(bytes, 0, read);
            }
            return read;
        }

        private void ProcessBytes(byte[] buffer, int offset, int count)
        {
            try
            {
                var text = Encoding.UTF8.GetString(buffer, offset, count);
                _buffer.Append(text);
                var full = _buffer.ToString();

                if (full.Contains("thought_signature"))
                {
                    var lines = full.Split('\n');
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        var line = lines[i].Trim();
                        if (line.StartsWith("data:"))
                        {
                            line = line.Substring(5).Trim();
                        }
                        if (line.StartsWith("{") && line.EndsWith("}"))
                        {
                            ExtractSignature(line);
                        }
                    }
                    _buffer.Clear();
                    _buffer.Append(lines[^1]);
                }
            }
            catch { }
        }

        private void ExtractSignature(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    JsonElement toolCalls = default;
                    if (choice.TryGetProperty("message", out var msg) && msg.TryGetProperty("tool_calls", out var tc1))
                        toolCalls = tc1;
                    else if (choice.TryGetProperty("delta", out var delta) && delta.TryGetProperty("tool_calls", out var tc2))
                        toolCalls = tc2;

                    if (toolCalls.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var tc in toolCalls.EnumerateArray())
                        {
                            var id = tc.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                            if (tc.TryGetProperty("extra_content", out var extra) &&
                                extra.TryGetProperty("google", out var google) &&
                                google.TryGetProperty("thought_signature", out var sig))
                            {
                                var sigStr = sig.GetString();
                                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(sigStr))
                                {
                                    _signatures[id] = sigStr;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await _inner.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
