using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        // 3. Capture thought_signature from response chunks/body
        if (message.Response?.ContentStream != null)
        {
            var responseMs = new MemoryStream();
            await message.Response.ContentStream.CopyToAsync(responseMs);
            var responseBytes = responseMs.ToArray();
            message.Response.ContentStream = new MemoryStream(responseBytes);

            var respStr = Encoding.UTF8.GetString(responseBytes);
            if (respStr.Contains("thought_signature"))
            {
                var lines = respStr.Split('\n');
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("data:"))
                    {
                        trimmed = trimmed.Substring(5).Trim();
                    }
                    if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(trimmed);
                            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                            {
                                var choice = choices[0];
                                JsonElement toolCallsElement = default;
                                if (choice.TryGetProperty("message", out var msg) && msg.TryGetProperty("tool_calls", out var tc1))
                                {
                                    toolCallsElement = tc1;
                                }
                                else if (choice.TryGetProperty("delta", out var delta) && delta.TryGetProperty("tool_calls", out var tc2))
                                {
                                    toolCallsElement = tc2;
                                }

                                if (toolCallsElement.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var tc in toolCallsElement.EnumerateArray())
                                    {
                                        var id = tc.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                                        if (tc.TryGetProperty("extra_content", out var extra) &&
                                            extra.TryGetProperty("google", out var google) &&
                                            google.TryGetProperty("thought_signature", out var sig))
                                        {
                                            var sigStr = sig.GetString();
                                            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(sigStr))
                                            {
                                                _thoughtSignatures[id] = sigStr;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
