using System.Text.Json.Serialization;

namespace AWS.NET.MCP.Utils;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(McpToolsManifest))]
internal partial class McpJsonContext : JsonSerializerContext
{
}
