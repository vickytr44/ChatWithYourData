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

        // 3. Extract thought signatures from response stream and reset seekable stream
        if (message.Response?.ContentStream != null)
        {
            using var ms = new MemoryStream();
            await message.Response.ContentStream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            ExtractSignaturesFromBytes(bytes, _thoughtSignatures);

            message.Response.ContentStream = new MemoryStream(bytes);
        }
    }

    private static void ExtractSignaturesFromBytes(byte[] bytes, ConcurrentDictionary<string, string> signatures)
    {
        if (bytes.Length == 0) return;
        try
        {
            var text = Encoding.UTF8.GetString(bytes);
            if (!text.Contains("thought_signature")) return;

            // Try direct JSON first (non-streaming calls like agent.RunAsync)
            try
            {
                using var doc = JsonDocument.Parse(bytes);
                ExtractSignatureFromDoc(doc.RootElement, signatures);
                return;
            }
            catch { }

            // Fallback for SSE lines (streaming calls like AG-UI SSE)
            var lines = text.Split('\n');
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.StartsWith("data:"))
                {
                    line = line.Substring(5).Trim();
                }
                if (line.StartsWith('{') && line.EndsWith('}'))
                {
                    try
                    {
                        using var lineDoc = JsonDocument.Parse(line);
                        ExtractSignatureFromDoc(lineDoc.RootElement, signatures);
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    private static void ExtractSignatureFromDoc(JsonElement root, ConcurrentDictionary<string, string> signatures)
    {
        if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0) return;

        foreach (var choice in choices.EnumerateArray())
        {
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
                            signatures[id] = sigStr;
                        }
                    }
                }
            }
        }
    }
}
